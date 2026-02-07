using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.User
{
    public class Role : IdentityRole<int>
    {
        public List<UserRole> UserRoles { get; set; } = new();
    }

}
