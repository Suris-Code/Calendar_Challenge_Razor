using Application.Common.Interfaces;
using Domain.Entities;
using Google.Authenticator;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue("UseInMemoryDatabase", false))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options => options
                   .UseSqlServer(
                       configuration.GetConnectionString("DefaultConnection"),
                       b =>
                       {
                           b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                           //b.CommandTimeout(configuration.GetValue<int>("EnvironmentConfiguration:DbTimeout", 60));
                       })
                   );

                services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

                #region JWT
                services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));
                services.AddScoped<JwtHandler>();

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwt =>
                {
                    var key = Encoding.ASCII.GetBytes(configuration.GetValue<string>("JwtConfig:Secret") ?? string.Empty);

                    jwt.SaveToken = true;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true, // this will validate the 3rd part of the jwt token using the secret that we added in the appsettings and verify we have generated the jwt token
                        IssuerSigningKey = new SymmetricSecurityKey(key), // Add the secret key to our Jwt encryption
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        //ValidIssuer = configuration["JwtConfig:Issuer"],
                        //ValidAudience = configuration["JwtConfig:Audience"],
                        //ValidAlgorithms = new[] { @"HS256" },
                        RequireExpirationTime = false,
                        ValidateLifetime = true
                    };
                });
                #endregion

                services.AddIdentity<ApplicationUser, ApplicationRole>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();

                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = true;
                });

                services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
                services.AddScoped<IUserActivityLogService, UserActivityLogService>();
                services.AddScoped<IActivityLogService, ActivityLogService>();
                services.AddScoped<IErrorLogService, ErrorLogService>();

                services.AddMediatR(opt => opt.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
                services.AddTransient<IDateTime, DateTimeService>();
                services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
                services.AddScoped<IEmailService, EmailService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddScoped<TwoFactorAuthenticator>();
                services.AddScoped<IAssetsService, AssetsService>();
            }

            return services;
        }
    }
}
