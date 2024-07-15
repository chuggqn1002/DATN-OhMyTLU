using System.ComponentModel.DataAnnotations;

namespace OhMyTLU.Models
{
    public class UserProfileViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? Description { get; set; }

        public byte[]? ImageData { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

}
