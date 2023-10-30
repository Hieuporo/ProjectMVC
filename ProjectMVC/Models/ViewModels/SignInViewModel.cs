using System.ComponentModel.DataAnnotations;

namespace ProjectMVC.Models.ViewModels
{
    public class SignInViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
