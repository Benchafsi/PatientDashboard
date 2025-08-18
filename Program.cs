using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatientDashboard.Data;
using PatientDashboard.RealTime;
using PatientDashboard.Services;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services
                .AddRazorPages(
                    options =>
                    {
                        // Lock everything by default
                        options.Conventions.AuthorizeFolder("/");
                        // We allow anonymous to Identity UI in order to register and login
                        options.Conventions.AllowAnonymousToFolder("/Identity");
                    });
            builder.Services.AddSingleton<VitalBroadcastInterceptor>();

            builder.Services
                .AddDbContext<PatientDashboardDbContext>(
                    (sp, options) =>
                    {
                        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
                            .AddInterceptors(sp.GetRequiredService<VitalBroadcastInterceptor>());
                    },
                    ServiceLifetime.Scoped);
            builder.Services
                .AddDefaultIdentity<IdentityUser>(
                    options =>
                    {
                        options.SignIn.RequireConfirmedAccount = true;
                    })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<PatientDashboardDbContext>();

            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IVitalSignService, VitalSignService>();
            builder.Services.AddSingleton<IVitalSimulationService, VitalSimulationService>();
            builder.Services.AddSignalR();
            var app = builder.Build();
            using(var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PatientDashboardDbContext>();
                var logger = app.Logger;

                try
                {
                    logger.LogInformation("Applying EF Core migrations�");
                    db.Database.Migrate();
                    DbSeeder. SeedAdminIdentity(scope.ServiceProvider, logger);
                    logger.LogInformation("Starting database seeding�");
                    DbSeeder.Seed(db);
                    logger.LogInformation("Database seeding completed.");
                } catch(Exception ex)
                {
                    logger.LogError(ex, "Fatal error during migration/seeding.");
                    throw;
                }
            }

            // Configure the HTTP request pipeline.
            if(!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapRazorPages();
            app.MapHub<VitalSignsFeedHub>("/hubs/vitals");

            app.Run();
        }
    }
}
