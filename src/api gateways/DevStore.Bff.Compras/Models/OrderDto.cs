using DevStore.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevStore.Bff.Compras.Models
{
    public class OrderDto
    {
        #region Pedido

        public int Code { get; set; }
        // Autorizado = 1,
        // Pago = 2,
        // Recusado = 3,
        // Entregue = 4,
        // Cancelado = 5
        public int Status { get; set; }
        public DateTime Data { get; set; }
        public decimal Amount { get; set; }

        public decimal Discount { get; set; }
        public string Voucher { get; set; }
        public bool HasVoucher { get; set; }

        public List<ShoppingCartItemDto> OrderItems { get; set; }

        #endregion

        #region Address

        public AddressDto Address { get; set; }

        #endregion

        #region Cartão

        [Required(ErrorMessage = "Card number is required")]
        [DisplayName("Card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Please inform card holder")]
        [DisplayName("Holder")]
        public string Holder { get; set; }

        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "The expiration must be in form of MM/YY")]
        [CartaoExpiracao(ErrorMessage = "Expired Credit Card")]
        [Required(ErrorMessage = "Credit card expiration is required")]
        [DisplayName("Expiration MM/YY")]
        public string ExpirationMonth { get; set; }

        [Required(ErrorMessage = "Security code is required")]
        [DisplayName("Security Code")]
        public string SecurityCode { get; set; }

        #endregion
    }
}