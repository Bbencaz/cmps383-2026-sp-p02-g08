using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using Selu383.SP26.Api.Features.User;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController(
    UserManager<User> userManager,
    SignInManager<User> signInManager
) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
            return BadRequest("Invalid username or password");

        var result = await signInManager.PasswordSignInAsync(
            user,
            loginDto.Password,
            isPersistent: false,
            lockoutOnFailure: false
        );

        if (!result.Succeeded)
            return BadRequest("Invalid username or password");

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new { Username = user.UserName, Id = user.Id, Roles = roles.ToArray() });
    }

    [HttpPost("logout")]
        public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized();
        var user = await userManager.GetUserAsync(User);
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = roles.ToArray()
        });
    }

}



public class LoginDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string[] Roles { get; set; }
}

