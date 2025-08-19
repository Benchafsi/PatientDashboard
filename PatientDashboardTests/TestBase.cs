using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientDashboard.Data;
using PatientDashboard.Services;

namespace PatientDashboardTests;

public abstract class TestBase : IDisposable
{
    protected readonly PatientDashboardDbContext DbContext;
    protected readonly Mock<ILogger<PatientService>> MockPatientLogger;
    protected readonly Mock<ILogger<VitalSignService>> MockVitalSignLogger;

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<PatientDashboardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new PatientDashboardDbContext(options);
        MockPatientLogger = new Mock<ILogger<PatientService>>();
        MockVitalSignLogger = new Mock<ILogger<VitalSignService>>();
        DbContext.Database.EnsureCreated();
    }

    protected async Task SeedDatabaseAsync() { await DbContext.SaveChangesAsync(); }

    public void Dispose() { DbContext?.Dispose(); }
}
