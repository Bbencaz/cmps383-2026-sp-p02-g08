using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.User
{
    public class UserRole : IdentityUserRole<int>
    {
        public User User { get; set; }
	    public Role Role { get; set; }
    }

}
