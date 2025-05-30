using Application;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Extensions;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true, reloadOnChange: true);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
            else
            {
                var frontendDomain = builder.Configuration.GetValue<string>("EnvironmentConfiguration:FrontendDomain")
                    ?? throw new Exception("FrontendDomain must be configured in production");

                policy.SetIsOriginAllowed(origin =>
                    origin.ToLower().StartsWith(frontendDomain))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("");
            }
        });

    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// MVC project - no need for Swagger/OpenAPI
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromDays(3));

ClaimPolicies.AddClaimPolicies(builder.Services);

builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<LogActionAttribute>();

var app = builder.Build();

#region Run migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var retry = builder.Configuration.GetValue("EnvironmentConfiguration:CountRetriesDBConnectionMigrations", 1);
    retry = retry < 1 ? 1 : retry;

    var runSeeds = builder.Configuration.GetValue("EnvironmentConfiguration:RunDBSeeds", false);
    bool runOk = false;
    Exception? lastException = null;

    for (int i = 0; i < retry && !runOk; i++)
    {
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            if (context.Database.IsSqlServer())
            {
                context.Database.Migrate();
            }

            if (runSeeds)
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                await ApplicationDbContextSeed.SeedBuiltInRolesAsync(roleManager);
                await ApplicationDbContextSeed.SeedBuiltInAdministratorAsync(userManager, context);
                await ApplicationDbContextSeed.SeedRandomAppointmentsAsync(context, userManager);
            }

            runOk = true;
        }
        catch (Exception ex)
        {
            lastException = ex;
        }
    }

    if (!runOk)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (lastException != null)
        {
            logger.LogError(lastException, "An error occurred while migrating or seeding the database.");
            throw lastException;
        }
        else
        {
            logger.LogError("An error occurred while migrating or seeding the database.");
            throw new Exception("An error occurred while migrating or seeding the database.");
        }
    }
}

#endregion

#region Security configurations
app.Use(async (context, next) =>
{

    context.Response.Headers.TryAdd("X-XSS-Protection", "0");
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "deny");
    //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; style-src 'self' https://fonts.googleapis.com; style-src-elem 'self' https://fonts.googleapis.com; img-src 'self' data:;");


    await next();
});
#endregion

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseExceptionHandling();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
