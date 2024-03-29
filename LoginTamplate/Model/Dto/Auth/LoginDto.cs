using System.ComponentModel.DataAnnotations;

namespace LoginTamplate.Model.Dto.Auth
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
