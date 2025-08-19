using Bogus;
using PatientDashboard.Models;

namespace PatientDashboardTests.TestData;

public static class PatientFaker
{
    public static Faker<Patient> CreatePatientFaker()
    {
        return new Faker<Patient>()
            .RuleFor(p => p.Id, f => f.Random.Int(1, 1000))
            .RuleFor(p => p.Name, f => f.Person.FullName)
            .RuleFor(p => p.Age, f => f.Random.Int(18, 95))
            .RuleFor(p => p.RoomNumber, f => f.Random.Replace("###-##"))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(2))
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent(30))
            .RuleFor(p => p.IsDeleted, false)
            .RuleFor(p => p.VitalSigns, new List<VitalSign>());
    }

    public static List<Patient> GeneratePatients(int count = 10)
    {
        return CreatePatientFaker().Generate(count);
    }

    public static Patient GeneratePatient()
    {
        return CreatePatientFaker().Generate();
    }
}
