using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace OhMyTLU.Data;



public partial class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
    public string Name { get; set; }
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; }

    public byte[]? Image { get; set; }

    public string? Description { get; set; }
    public bool IsOnline { get; set; } = false;

    public bool IsAdmin { get; set; } = false;
}
