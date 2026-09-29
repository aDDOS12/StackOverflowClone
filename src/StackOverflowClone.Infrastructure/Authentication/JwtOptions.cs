using System.ComponentModel.DataAnnotations;

namespace StackOverflowClone.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required]
    public string Issuer { get; set; } = default!;
    [Required]
    public string Audience { get; set; } = default!;
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = default!;
    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;
}
