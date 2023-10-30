using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectMVC.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
        public string Brand { get; set; }
        [Required]
        [Range(1, 1000)]
        public double Price { get; set; }
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }

    }
}
