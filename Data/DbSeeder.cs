using Microsoft.AspNetCore.Identity;
using PatientDashboard.Models;

namespace PatientDashboard.Data;

public static class DbSeeder
{
    public static void Seed(PatientDashboardDbContext context)
    {
        if(!context.Patients.Any())
        {
            var patients = new List<Patient>
            {
                new() { Name = "John Doe", Age = 45, RoomNumber = "101" },
                new() { Name = "Jane Smith", Age = 32, RoomNumber = "102" },
                new() { Name = "Bob Johnson", Age = 67, RoomNumber = "103" }
            };

            context.Patients.AddRange(patients);
            context.SaveChanges();

            var now = DateTime.UtcNow;

            var vitals = new List<VitalSign>
            {
                new()
                {
                    PatientId = patients[0].Id,
                    HeartRate = 88,
                    Systolic = 118,
                    Diastolic = 78,
                    OxygenSaturation = 97,
                    MeasuredAt = now
                },
                new()
                {
                    PatientId = patients[1].Id,
                    HeartRate = 105,
                    Systolic = 125,
                    Diastolic = 85,
                    OxygenSaturation = 94,
                    MeasuredAt = now
                },
                new()
                {
                    PatientId = patients[2].Id,
                    HeartRate = 130,
                    Systolic = 142,
                    Diastolic = 92,
                    OxygenSaturation = 89,
                    MeasuredAt = now
                }
            };

            context.VitalSigns.AddRange(vitals);
            context.SaveChanges();
        }
    }

    public static void SeedAdminIdentity(IServiceProvider services, ILogger logger)
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        const string role = "Admin";
        const string email = "admin@test.com";
        const string pwd = "Pass12345!@#$%";

        if(!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            logger.LogInformation("Created role {Role}", role);
        }

        var user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
        if(user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            userManager.CreateAsync(user, pwd).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(user, role).GetAwaiter().GetResult();
            logger.LogInformation("Seeded default admin {Email}", email);
        }
    }
}