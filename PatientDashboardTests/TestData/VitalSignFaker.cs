using Bogus;
using PatientDashboard.Models;
using PatientDashboard.Models.Enums;

namespace PatientDashboardTests.TestData;

public static class VitalSignFaker
{
    public static Faker<VitalSign> CreateVitalSignFaker(int? patientId = null)
    {
        return new Faker<VitalSign>()
            .RuleFor(v => v.Id, f => f.Random.Int(1, 1000))
            .RuleFor(v => v.HeartRate, f => f.Random.Int(50, 150))
            .RuleFor(v => v.Systolic, f => f.Random.Int(80, 180))
            .RuleFor(v => v.Diastolic, f => f.Random.Int(50, 120))
            .RuleFor(v => v.OxygenSaturation, f => f.Random.Int(85, 100))
            .RuleFor(v => v.MeasuredAt, f => f.Date.Recent(7))
            .RuleFor(v => v.PatientId, f => patientId ?? f.Random.Int(1, 100))
            .RuleFor(v => v.Status, VitalStatus.Normal)
            .RuleFor(v => v.CreatedAt, f => f.Date.Past(1))
            .RuleFor(v => v.UpdatedAt, f => f.Date.Recent(30))
            .RuleFor(v => v.IsDeleted, false)
            .RuleFor(v => v.Patient, (f, v) => null!);
    }

    public static List<VitalSign> GenerateVitalSigns(int count = 10, int? patientId = null)
    {
        return CreateVitalSignFaker(patientId).Generate(count);
    }

    public static VitalSign GenerateVitalSign(int? patientId = null)
    {
        return CreateVitalSignFaker(patientId).Generate();
    }

    public static VitalSign GenerateCriticalVitalSign(int? patientId = null)
    {
        return CreateVitalSignFaker(patientId)
            .RuleFor(v => v.HeartRate, f => f.Random.Int(130, 180))
            .RuleFor(v => v.Systolic, f => f.Random.Int(150, 200))
            .RuleFor(v => v.Diastolic, f => f.Random.Int(100, 130))
            .RuleFor(v => v.OxygenSaturation, f => f.Random.Int(75, 89))
            .Generate();
    }

    public static VitalSign GenerateWarningVitalSign(int? patientId = null)
    {
        return CreateVitalSignFaker(patientId)
            .RuleFor(v => v.HeartRate, f => f.Random.Int(100, 129))
            .RuleFor(v => v.Systolic, f => f.Random.Int(120, 149))
            .RuleFor(v => v.Diastolic, f => f.Random.Int(80, 99))
            .RuleFor(v => v.OxygenSaturation, f => f.Random.Int(90, 95))
            .Generate();
    }

    public static VitalSign GenerateNormalVitalSign(int? patientId = null)
    {
        return CreateVitalSignFaker(patientId)
            .RuleFor(v => v.HeartRate, f => f.Random.Int(60, 99))
            .RuleFor(v => v.Systolic, f => f.Random.Int(90, 119))
            .RuleFor(v => v.Diastolic, f => f.Random.Int(60, 79))
            .RuleFor(v => v.OxygenSaturation, f => f.Random.Int(96, 100))
            .Generate();
    }
}
