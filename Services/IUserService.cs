using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByIdAsync(Guid id);

    Task<User> CreateAsync(User user);

    Task<User?> UpdateAsync(Guid id, User user);

    Task<bool> DeleteAsync(Guid id);
}