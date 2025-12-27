using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs;

public sealed class UserCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; init; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;
}