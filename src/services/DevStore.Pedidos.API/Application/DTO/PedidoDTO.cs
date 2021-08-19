using System;
using System.Collections.Generic;
using DevStore.Pedidos.Domain.Pedidos;

namespace DevStore.Pedidos.API.Application.DTO
{
    public class PedidoDTO
    {
        public Guid Id { get; set; }
        public int Codigo { get; set; }

        public Guid ClienteId { get; set; }
        public int Status { get; set; }
        public DateTime Data { get; set; }
        public decimal ValorTotal { get; set; }

        public decimal Desconto { get; set; }
        public string VoucherCodigo { get; set; }
        public bool VoucherUtilizado { get; set; }

        public List<PedidoItemDTO> PedidoItems { get; set; }
        public AddressDto Address { get; set; }

        public static PedidoDTO ParaPedidoDTO(Pedido pedido)
        {
            var pedidoDTO = new PedidoDTO
            {
                Id = pedido.Id,
                Codigo = pedido.Codigo,
                Status = (int)pedido.PedidoStatus,
                Data = pedido.DataCadastro,
                ValorTotal = pedido.ValorTotal,
                Desconto = pedido.Desconto,
                VoucherUtilizado = pedido.VoucherUtilizado,
                PedidoItems = new List<PedidoItemDTO>(),
                Address = new AddressDto()
            };

            foreach (var item in pedido.PedidoItems)
            {
                pedidoDTO.PedidoItems.Add(new PedidoItemDTO
                {
                    Nome = item.ProdutoNome,
                    Imagem = item.ProdutoImagem,
                    Quantidade = item.Quantidade,
                    ProdutoId = item.ProdutoId,
                    Valor = item.ValorUnitario,
                    PedidoId = item.PedidoId
                });
            }

            pedidoDTO.Address = new AddressDto
            {
                StreetAddress = pedido.Endereco.Logradouro,
                BuildingNumber = pedido.Endereco.Numero,
                SecondaryAddress = pedido.Endereco.Complemento,
                Neighborhood = pedido.Endereco.Bairro,
                ZipCode = pedido.Endereco.Cep,
                City = pedido.Endereco.Cidade,
                State = pedido.Endereco.Estado,
            };

            return pedidoDTO;
        }
    }
}