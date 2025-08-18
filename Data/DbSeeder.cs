using Microsoft.AspNetCore.Identity;
using PatientDashboard.Models;
using PatientDashboard.Models.Enums;

namespace PatientDashboard.Data;

public static class DbSeeder
{
    private static readonly Random _rng = new();

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
        }

        if(!context.VitalSigns.Any())
        {
            var now = DateTime.UtcNow;
            var patients = context.Patients.ToList();
            var allVitals = new List<VitalSign>();

            foreach(var p in patients)
            {
                // exactly 6 Normal, 3 Warning, 1 Critical -> 10 total per patient
                var desired = new[]
                {
                    VitalStatus.Normal,
                    VitalStatus.Normal,
                    VitalStatus.Normal,
                    VitalStatus.Normal,
                    VitalStatus.Normal,
                    VitalStatus.Normal,
                    VitalStatus.Warning,
                    VitalStatus.Warning,
                    VitalStatus.Warning,
                    VitalStatus.Critical
                };

                for(int i = 0; i < desired.Length; i++)
                {
                    // spaced at 30s apart in the past
                    var t = now.AddSeconds(-30 * (desired.Length - i));
                    var v = MakeVitalForStatus(p.Id, t, desired[i]);
                    v.Status = CalculateStatus(v);
                    allVitals.Add(v);
                }
            }

            context.VitalSigns.AddRange(allVitals);
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

    private static VitalSign MakeVitalForStatus(int patientId, DateTime measuredAt, VitalStatus status)
    {
        int hr, spo2, sys, dia;

        switch(status)
        {
            case VitalStatus.Normal:
                hr = Rand(60, 100);
                spo2 = Rand(96, 100);            // >95
                (sys, dia) = BpNormal();
                break;

            case VitalStatus.Warning:
                // choose a warning dimension randomly
                var wPick = Rand(0, 2);
                if(wPick == 0)
                {
                    hr = Rand(105, 120);         // HR warning
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpNormal();
                } else if(wPick == 1)
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(90, 95);         // SpO2 warning
                    (sys, dia) = BpNormal();
                } else
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpWarning();    // BP warning
                }
                break;

            case VitalStatus.Critical:
            default:
                var cPick = Rand(0, 2);
                if(cPick == 0)
                {
                    hr = Rand(130, 160);         // HR critical
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpNormal();
                } else if(cPick == 1)
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(85, 89);         // SpO2 critical
                    (sys, dia) = BpNormal();
                } else
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpCritical();   // BP critical
                }
                break;
        }

        return new VitalSign
        {
            PatientId = patientId,
            MeasuredAt = measuredAt,
            HeartRate = hr,
            OxygenSaturation = spo2,
            Systolic = sys,
            Diastolic = dia
        };
    }

    private static (int sys, int dia) BpNormal()
    {
        var sys = Rand(95, 119);
        var dia = Rand(60, 79);
        dia = Math.Min(dia, sys - 20);
        return (sys, Math.Max(50, dia));
    }

    private static (int sys, int dia) BpWarning()
    {
        // either systolic 120-139 or diastolic 80-89
        var choose = Rand(0, 1);
        int sys, dia;
        if(choose == 0)
        {
            sys = Rand(120, 139);
            dia = Rand(70, 89);
        } else
        {
            sys = Rand(110, 139);
            dia = Rand(80, 89);
        }
        dia = Math.Min(dia, sys - 20);
        return (sys, Math.Max(50, dia));
    }

    private static (int sys, int dia) BpCritical()
    {
        // systolic >= 140 or diastolic >= 91
        var choose = Rand(0, 1);
        int sys, dia;
        if(choose == 0)
        {
            sys = Rand(140, 180);
            dia = Rand(80, 100);
        } else
        {
            sys = Rand(130, 170);
            dia = Rand(91, 110);
        }
        dia = Math.Min(dia, sys - 20);
        return (sys, Math.Max(50, dia));
    }

    private static int Rand(int min, int maxInclusive) => _rng.Next(min, maxInclusive + 1);

    private static VitalStatus CalculateStatus(VitalSign v)
    {
        var hr = v.HeartRate switch
        {
            > 120 => VitalStatus.Critical,
            >= 100 => VitalStatus.Warning,
            < 60 => VitalStatus.Warning,
            _ => VitalStatus.Normal
        };

        var bp = (v.Systolic > 139 || v.Diastolic > 90)
            ? VitalStatus.Critical
            : (v.Systolic >= 120 || v.Diastolic >= 80)
                ? VitalStatus.Warning
                : (v.Systolic >= 90 && v.Diastolic >= 60) ? VitalStatus.Normal : VitalStatus.Warning;

        var ox = v.OxygenSaturation < 90
            ? VitalStatus.Critical
            : v.OxygenSaturation <= 95 ? VitalStatus.Warning : VitalStatus.Normal;

        if(hr == VitalStatus.Critical || bp == VitalStatus.Critical || ox == VitalStatus.Critical)
            return VitalStatus.Critical;
        if(hr == VitalStatus.Warning || bp == VitalStatus.Warning || ox == VitalStatus.Warning)
            return VitalStatus.Warning;
        return VitalStatus.Normal;
    }
}