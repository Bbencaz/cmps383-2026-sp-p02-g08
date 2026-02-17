namespace Selu383.SP26.Api.Dtos;

public class CreateUserDto
{
    public string UserName { get; set; } = "";
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string Password { get; set; } = "";
}
