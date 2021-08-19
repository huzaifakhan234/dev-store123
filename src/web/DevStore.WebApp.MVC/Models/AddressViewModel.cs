using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevStore.WebApp.MVC.Models
{
    public class AddressViewModel
    {
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        [DisplayName("Número")]
        public string BuildingNumber { get; set; }
        public string SecondaryAddress { get; set; }
        [Required]
        public string Neighborhood { get; set; }
        [Required]
        [DisplayName("Zip Code")]
        public string ZipCode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }

        public override string ToString()
        {
            return $"{StreetAddress}, {BuildingNumber} {SecondaryAddress} - {Neighborhood} - {City} - {State}";
        }
    }
}