using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Services;

public sealed class InMemoryUserService : IUserService
{
    private readonly Dictionary<Guid, User> _store = new();
    private readonly object _lock = new();

    public Task<IEnumerable<User>> GetAllAsync()
    {
        lock (_lock)
        {
            var users = _store.Values.OrderBy(u => u.CreatedAt).ToArray();
            return Task.FromResult<IEnumerable<User>>(users);
        }
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            _store.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
    }

    public Task<User> CreateAsync(User user)
    {
        var toAdd = user with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        lock (_lock)
        {
            _store[toAdd.Id] = toAdd;
        }

        return Task.FromResult(toAdd);
    }

    public Task<User?> UpdateAsync(Guid id, User user)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(id, out var existing))
            {
                return Task.FromResult<User?>(null);
            }

            var updated = existing with
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UpdatedAt = DateTime.UtcNow
            };

            _store[id] = updated;
            return Task.FromResult<User?>(updated);
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.Remove(id));
        }
    }
}