using System.ComponentModel.DataAnnotations;

namespace ProjectMVC.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
		[Required]
		public string Address { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
