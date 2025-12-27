using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.DTOs;
using UserManagement.Models;
using UserManagement.Services;

namespace UserManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) =>
        _userService = userService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        var dtos = users.Select(u => ToReadDto(u));
        return Ok(dtos);
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    public async Task<ActionResult<UserReadDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserReadDto>> Create([FromBody] UserCreateDto create)
    {
        var userModel = new User
        {
            Id = Guid.Empty, // service will assign
            FirstName = create.FirstName,
            LastName = create.LastName,
            Email = create.Email,
            CreatedAt = DateTime.MinValue
        };

        var created = await _userService.CreateAsync(userModel);
        var dto = ToReadDto(created);

        return CreatedAtRoute("GetUserById", new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserReadDto>> Update(Guid id, [FromBody] UserUpdateDto update)
    {
        var userModel = new User
        {
            Id = id,
            FirstName = update.FirstName,
            LastName = update.LastName,
            Email = update.Email,
            CreatedAt = DateTime.MinValue
        };

        var updated = await _userService.UpdateAsync(id, userModel);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var removed = await _userService.DeleteAsync(id);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static UserReadDto ToReadDto(User u) =>
        new()
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };
}