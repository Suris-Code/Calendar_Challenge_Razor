using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Infrastructure.Identity
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        public AppClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
                                         RoleManager<ApplicationRole> roleManager,
                                         IOptions<IdentityOptions> optionsAccessor
                                        ) : base(userManager, roleManager, optionsAccessor) { }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim("FirstName", user.FirstName.ToString()) });
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim("LastName", user.LastName.ToString()) });
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim("UserName", user.UserName.ToString()) });
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim("Email", user.Email.ToString()) });

            return principal;
        }
    }

}
