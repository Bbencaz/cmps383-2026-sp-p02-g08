using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Dtos;
using Selu383.SP26.Api.Features.User;

namespace Selu383.SP26.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController(
    UserManager<User> userManager,
    RoleManager<Role> roleManager
) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create(CreateUserDto dto)
    {
        var userName = (dto.UserName ?? "").Trim();

        if (string.IsNullOrEmpty(userName))
        
            return BadRequest("UserName is required.");
        

        if (string.IsNullOrEmpty(dto.Password))
            return BadRequest("Password is required.");

        if (dto.Roles == null || dto.Roles.Length == 0)
            return BadRequest("At least one role is required.");

        var roles = dto.Roles
            .Where(r => !string.IsNullOrEmpty(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles.Length == 0)
            return BadRequest("At least one role is required.");

        var existingUser = await userManager.FindByNameAsync(userName);

        if (existingUser != null)
            return BadRequest("Username already exists.");

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                return BadRequest($"Role not found: {role}");
        }

        var user = new User { UserName = userName };

        var createResult = await userManager.CreateAsync(user, dto.Password);

        if (!createResult.Succeeded)
            return BadRequest(createResult.Errors.Select(e => e.Description));

        var addRolesResult = await userManager.AddToRolesAsync(user, roles);

        if (!addRolesResult.Succeeded)
            return BadRequest(addRolesResult.Errors.Select(e => e.Description));

        return Ok(new { Username = user.UserName, Id = user.Id, Roles = roles });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetById(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user == null)
            return NotFound();

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new { Username = user.UserName, Id = user.Id, Roles = roles.ToArray() });
    }
}
