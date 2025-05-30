using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedBuiltInRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            #region Admin 
            if (await roleManager.FindByNameAsync(Role.Admin.ToString()) == null)
            {
                var adminRole = new ApplicationRole
                {
                    Name = Role.Admin.ToString(),
                    NormalizedName = Role.Admin.ToString().ToUpper(),
                    Created = DateTime.Now,
                    CreatedBy = "setup",
                    LastModified = DateTime.Now,
                    LastModifiedBy = "setup"
                };
                await roleManager.CreateAsync(adminRole);
                await roleManager.AddClaimAsync(adminRole, new Claim(PolicyClaim.LoggedIn.ToString(), "true"));
                await roleManager.AddClaimAsync(adminRole, new Claim(PolicyClaim.Admin.ToString(), "true"));
            }
            #endregion

            #region User

            if (await roleManager.FindByNameAsync(Role.User.ToString()) == null)
            {
                var standardRole = new ApplicationRole
                {
                    Name = Role.User.ToString(),
                    NormalizedName = Role.User.ToString().ToUpper(),
                    Created = DateTime.Now,
                    CreatedBy = "setup",
                    LastModified = DateTime.Now,
                    LastModifiedBy = "setup"
                };

                await roleManager.CreateAsync(standardRole);
                await roleManager.AddClaimAsync(standardRole, new Claim(PolicyClaim.LoggedIn.ToString(), "true"));
            }
            #endregion
        }

        public static async Task SeedBuiltInAdministratorAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var adminUsername = "candidato@suriscode.com";

            if (!userManager.Users.Any(x => x.UserName == adminUsername))
            {
                #region Add the system administrator

                ApplicationUser adminUser = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = adminUsername,
                    FirstName = "Candidato",
                    LastName = "ReactNet",
                    EmailConfirmed = true,
                    Enabled = YesNo.Yes,
                    Created = DateTime.Now,
                    CreatedBy = "setup",
                    LastModified = DateTime.Now,
                    LastModifiedBy = "setup",
                };
                var userResult = await userManager.CreateAsync(adminUser, "Suris-challenge-2025");

                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Role.Admin.ToString());
                }
                #endregion   
            }

            var privateUsername = "private@suriscode.com";

            if (!userManager.Users.Any(x => x.UserName == privateUsername))
            {
                #region Add the private user

                ApplicationUser privateUser = new ApplicationUser
                {
                    UserName = privateUsername,
                    Email = privateUsername,
                    FirstName = "Private",
                    LastName = "ReactNet",
                    EmailConfirmed = true,
                    Enabled = YesNo.Yes,
                    Created = DateTime.Now,
                    CreatedBy = "setup",
                    LastModified = DateTime.Now,
                    LastModifiedBy = "setup",
                };
                var userResult = await userManager.CreateAsync(privateUser, "Private-2025");

                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(privateUser, Role.User.ToString());
                }
                #endregion   
            }
        }

        public static async Task SeedRandomAppointmentsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Skip if we already have appointments
            if (await context.Appointments.AnyAsync())
                return;
                
            // Get users to assign appointments to
            var users = await userManager.Users.Where(u => u.Enabled == YesNo.Yes).ToListAsync();
            if (!users.Any())
                return;

            // Reference date (today)
            var today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            
            // Time range: last month to next 2 months
            var startDate = today.AddMonths(-1);
            var endDate = today.AddMonths(2);
            
            // Random generator
            var random = new Random();
            
            // Possible appointment titles
            var titles = new[]
            {
                "Team Meeting", 
                "Client Consultation", 
                "Project Review",
                "Training Session",
                "Interview",
                "Planning Session",
                "Demo",
                "Presentation",
                "Status Update",
                "Performance Review"
            };
            
            // Possible locations
            var locations = new[]
            {
                "Conference Room A",
                "Conference Room B",
                "Virtual Meeting",
                "Client Office",
                "Main Office",
                "Meeting Room 101",
                "Training Center",
                null
            };

            // Create appointments for each day
            var currentDate = startDate;
            var appointments = new List<Appointment>();
            
            while (currentDate <= endDate)
            {
                // Generate 0-5 appointments per day
                int appointmentsCount = random.Next(6); // 0 to 5
                
                for (int i = 0; i < appointmentsCount; i++)
                {
                    // Random hour between 8 AM and 5 PM
                    int hour = random.Next(8, 18);
                    
                    // Duration between 30 mins and 2 hours
                    int durationMinutes = random.Next(1, 5) * 30;
                    
                    // Make sure the appointment doesn't go beyond 18:00
                    var startTime = currentDate.AddHours(hour);
                    var endTime = startTime.AddMinutes(durationMinutes);
                    
                    // Skip this appointment if it would end after 18:00
                    if (endTime.Hour >= 18 && endTime.Minute > 0)
                    {
                        continue;
                    }
                    
                    // Generate the appointment
                    var appointment = new Appointment
                    {
                        Title = titles[random.Next(titles.Length)],
                        Description = $"Description for {titles[random.Next(titles.Length)]} on {currentDate.ToString("yyyy-MM-dd")}",
                        StartTime = startTime,
                        EndTime = endTime,
                        Location = locations[random.Next(locations.Length)],
                        UserId = users[random.Next(users.Count)].Id,
                        IsConfirmed = random.Next(100) < 70 ? YesNo.Yes : YesNo.No, // 70% are confirmed
                        IsCancelled = random.Next(100) < 10 ? YesNo.Yes : YesNo.No, // 10% are cancelled
                        CancellationReason = null,
                        MeetingLink = random.Next(100) < 40 ? "https://meet.example.com/" + Guid.NewGuid().ToString("N").Substring(0, 8) : null, // 40% have meeting links
                        SendReminder = random.Next(100) < 80 ? YesNo.Yes : YesNo.No, // 80% have reminders
                        ReminderSentAt = null,
                        Created = today,
                        CreatedBy = "setup",
                        LastModified = today,
                        LastModifiedBy = "setup"
                    };
                    
                    // Set cancellation reason for cancelled appointments
                    if (appointment.IsCancelled == YesNo.Yes)
                    {
                        appointment.CancellationReason = "Cancelled due to scheduling conflict";
                    }
                    
                    // Set reminder sent time for past appointments with reminders
                    if (appointment.StartTime < today && appointment.SendReminder == YesNo.Yes)
                    {
                        appointment.ReminderSentAt = appointment.StartTime.AddHours(-24); // 24 hours before start
                    }
                    
                    appointments.Add(appointment);
                }
                
                currentDate = currentDate.AddDays(1);
            }
            
            // Add all appointments to the context
            await context.Appointments.AddRangeAsync(appointments);
            await context.SaveChangesAsync();
        }
    }
}

