using System.ComponentModel.DataAnnotations;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class RegisterUserDto
    {
        //add id
        [Required]
        public int Id { get; set; }
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
        [Required]
        public string? FirebaseToken { get; set; }
    }
}
