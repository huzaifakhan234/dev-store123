using DevStore.Bff.Compras.Models;
using DevStore.Bff.Compras.Services;
using DevStore.Bff.Compras.Services.gRPC;
using DevStore.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevStore.Bff.Compras.Controllers
{
    [Authorize, Route("orders/shopping-cart")]
    public class ShoppingCartController : MainController
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShoppingCartGrpcService _shoppingCartGrpcService;
        private readonly ICatalogService _catalogService;
        private readonly IOrderService _orderService;

        public ShoppingCartController(
            IShoppingCartService shoppingCartService,
            IShoppingCartGrpcService shoppingCartGrpcService,
            ICatalogService catalogService,
            IOrderService orderService)
        {
            _shoppingCartService = shoppingCartService;
            _shoppingCartGrpcService = shoppingCartGrpcService;
            _catalogService = catalogService;
            _orderService = orderService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            return CustomResponse(await _shoppingCartGrpcService.GetShoppingCart());
        }

        [HttpGet]
        [Route("quantity")]
        public async Task<int> GetCartItemsQuantity()
        {
            var quantidade = await _shoppingCartGrpcService.GetShoppingCart();
            return quantidade?.Items.Sum(i => i.Quantity) ?? 0;
        }

        [HttpPost]
        [Route("items")]
        public async Task<IActionResult> AddItem(ShoppingCartItemDto shoppingCartItemProduto)
        {
            var produto = await _catalogService.GetById(shoppingCartItemProduto.ProductId);

            await ValidateShoppingCartItem(produto, shoppingCartItemProduto.Quantity, true);
            if (!ValidOperation()) return CustomResponse();

            shoppingCartItemProduto.Name = produto.Name;
            shoppingCartItemProduto.Price = produto.Price;
            shoppingCartItemProduto.Image = produto.Image;

            var resposta = await _shoppingCartService.AddItem(shoppingCartItemProduto);

            return CustomResponse(resposta);
        }

        [HttpPut]
        [Route("items/{produtoId}")]
        public async Task<IActionResult> UpdateCartIem(Guid produtoId, ShoppingCartItemDto shoppingCartItemProduto)
        {
            var produto = await _catalogService.GetById(produtoId);

            await ValidateShoppingCartItem(produto, shoppingCartItemProduto.Quantity);
            if (!ValidOperation()) return CustomResponse();

            var resposta = await _shoppingCartService.UpdateItem(produtoId, shoppingCartItemProduto);

            return CustomResponse(resposta);
        }

        [HttpDelete]
        [Route("items/{produtoId}")]
        public async Task<IActionResult> RemoveItem(Guid produtoId)
        {
            var produto = await _catalogService.GetById(produtoId);

            if (produto == null)
            {
                AddErrorToStack("Product not found!");
                return CustomResponse();
            }

            var resposta = await _shoppingCartService.RemoveItem(produtoId);

            return CustomResponse(resposta);
        }

        [HttpPost]
        [Route("apply-voucher")]
        public async Task<IActionResult> ApplyVoucher([FromBody] string voucherCodigo)
        {
            var voucher = await _orderService.GetVoucherByCode(voucherCodigo);
            if (voucher is null)
            {
                AddErrorToStack("Voucher is invalid or not found!");
                return CustomResponse();
            }

            var resposta = await _shoppingCartService.ApplyVoucher(voucher);

            return CustomResponse(resposta);
        }

        private async Task ValidateShoppingCartItem(ProductDto product, int quantity, bool addProduct = false)
        {
            if (product == null) AddErrorToStack("Product not found!");
            if (quantity < 1) AddErrorToStack($"Should have at least one unit of product {product.Name}");

            var carrinho = await _shoppingCartService.GetShoppingCart();
            var itemCarrinho = carrinho.Items.FirstOrDefault(p => p.ProductId == product.Id);

            if (itemCarrinho != null && addProduct && itemCarrinho.Quantity + quantity > product.Stock)
            {
                AddErrorToStack($"The product {product.Name} has {product.Stock} units at stock, you got {quantity}");
                return;
            }

            if (quantity > product.Stock) AddErrorToStack($"The product {product.Name} has {product.Stock} units at stock, you got {quantity}");
        }
    }
}
