using Domain.Enums;

namespace Web.Services
{
    public static class ClaimPolicies
    {
        public static void AddClaimPolicies(IServiceCollection services)
        {
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy(Policy.LoggedIn.ToString(), p =>
                {
                    p.RequireClaim(nameof(PolicyClaim.LoggedIn), "true");
                });

                opts.AddPolicy(Policy.Admin.ToString(), p =>
                {
                    p.RequireClaim(nameof(PolicyClaim.Admin), "true");
                });

                opts.AddPolicy(Policy.AppointmentOwner.ToString(), p =>
                {
                    p.RequireClaim(nameof(PolicyClaim.AppointmentOwner), "true");
                });
            });
        }
    }
} 