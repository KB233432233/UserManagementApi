using System;

namespace UserManagement.Models;

public sealed class User
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}