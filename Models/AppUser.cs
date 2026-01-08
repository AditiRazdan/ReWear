using System.ComponentModel.DataAnnotations;

namespace LocalBakery.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string UserName { get; set; } = "";

    [Required, MaxLength(64)]
    public string NormalizedUserName { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Customer";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
