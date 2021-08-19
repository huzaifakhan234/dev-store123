using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using MediatR;
using DevStore.Core.Messages;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus;
using DevStore.Pedidos.API.Application.DTO;
using DevStore.Pedidos.API.Application.Events;
using DevStore.Pedidos.Domain;
using DevStore.Pedidos.Domain.Pedidos;
using DevStore.Pedidos.Domain.Specs;

namespace DevStore.Pedidos.API.Application.Commands
{
    public class PedidoCommandHandler : CommandHandler,
        IRequestHandler<AddOrderCommand, ValidationResult>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMessageBus _bus;

        public PedidoCommandHandler(IVoucherRepository voucherRepository, 
                                    IPedidoRepository pedidoRepository, 
                                    IMessageBus bus)
        {
            _voucherRepository = voucherRepository;
            _pedidoRepository = pedidoRepository;
            _bus = bus;
        }

        public async Task<ValidationResult> Handle(AddOrderCommand message, CancellationToken cancellationToken)
        {
            // Validação do comando
            if (!message.IsValid()) return message.ValidationResult;

            // Mapear Pedido
            var pedido = MapearPedido(message);

            // Aplicar voucher se houver
            if (!await AplicarVoucher(message, pedido)) return ValidationResult;

            // Validate pedido
            if (!ValidarPedido(pedido)) return ValidationResult;

            // Processar pagamento
            if (!await ProcessarPagamento(pedido, message)) return ValidationResult;

            // Se pagamento tudo ok!
            pedido.AutorizarPedido();

            // Adicionar Evento
            pedido.AddEvent(new PedidoRealizadoEvent(pedido.Id, pedido.ClienteId));

            // Adicionar Pedido Repositorio
            _pedidoRepository.Adicionar(pedido);

            // Persistir dados de pedido e voucher
            return await PersistData(_pedidoRepository.UnitOfWork);
        }

        private Pedido MapearPedido(AddOrderCommand message)
        {
            var endereco = new Endereco
            {
                Logradouro = message.Address.StreetAddress,
                Numero = message.Address.BuildingNumber,
                Complemento = message.Address.SecondaryAddress,
                Bairro = message.Address.Neighborhood,
                Cep = message.Address.ZipCode,
                Cidade = message.Address.City,
                Estado = message.Address.State
            };

            var pedido = new Pedido(message.ClientId, message.Amount, message.PedidoItems.Select(PedidoItemDTO.ParaPedidoItem).ToList(),
                message.HasVoucher, message.Discount);

            pedido.AtribuirEndereco(endereco);
            return pedido;
        }

        private async Task<bool> AplicarVoucher(AddOrderCommand message, Pedido pedido)
        {
            if (!message.HasVoucher) return true;

            var voucher = await _voucherRepository.ObterVoucherPorCodigo(message.Voucher);
            if (voucher == null)
            {
                AdicionarErro("O voucher informado não existe!");
                return false;
            }

            var voucherValidation = new VoucherValidation().Validate(voucher);
            if (!voucherValidation.IsValid)
            {
                voucherValidation.Errors.ToList().ForEach(m => AdicionarErro(m.ErrorMessage));
                return false;
            }

            pedido.AtribuirVoucher(voucher);
            voucher.DebitarQuantidade();

            _voucherRepository.Atualizar(voucher);

            return true;
        }

        private bool ValidarPedido(Pedido pedido)
        {
            var pedidoValorOriginal = pedido.ValorTotal;
            var pedidoDesconto = pedido.Desconto;

            pedido.CalcularValorPedido();

            if (pedido.ValorTotal != pedidoValorOriginal)
            {
                AdicionarErro("O valor total do pedido não confere com o cálculo do pedido");
                return false;
            }

            if (pedido.Desconto != pedidoDesconto)
            {
                AdicionarErro("O valor total não confere com o cálculo do pedido");
                return false;
            }

            return true;
        }

        public async Task<bool> ProcessarPagamento(Pedido pedido, AddOrderCommand message)
        {
            var pedidoIniciado = new PedidoIniciadoIntegrationEvent
            {
                PedidoId = pedido.Id,
                ClienteId = pedido.ClienteId,
                Valor = pedido.ValorTotal,
                TipoPagamento = 1, // fixo. Alterar se tiver mais tipos
                NomeCartao = message.Holder,
                NumeroCartao = message.CardNumber,
                MesAnoVencimento = message.ExpirationMonth,
                CVV = message.SecurityCode
            };

            var result = await _bus
                .RequestAsync<PedidoIniciadoIntegrationEvent, ResponseMessage>(pedidoIniciado);
            
            if (result.ValidationResult.IsValid) return true;

            foreach (var erro in result.ValidationResult.Errors)
            {
                AdicionarErro(erro.ErrorMessage);
            }

            return false;
        }
    }
}