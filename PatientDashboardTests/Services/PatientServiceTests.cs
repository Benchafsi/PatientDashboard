using PatientDashboard.Models;
using PatientDashboard.Services;
using PatientDashboardTests.TestData;

namespace PatientDashboardTests.Services;

public class PatientServiceTests : TestBase
{
    private readonly PatientService _patientService;

    public PatientServiceTests() { _patientService = new PatientService(DbContext, MockPatientLogger.Object); }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPatients_WhenIncludeVitalsIsFalse()
    {
        // Arrange
        var patients = PatientFaker.GeneratePatients(5);
        await DbContext.Patients.AddRangeAsync(patients);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _patientService.GetAllAsync(includeVitals: false);

        // Assert
        result.Should().HaveCount(5);
        result.Should().NotContain(p => p.VitalSigns.Any());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPatientsWithVitals_WhenIncludeVitalsIsTrue()
    {
        // Arrange
        var patients = PatientFaker.GeneratePatients(3);
        patients[0].Id = 1;
        patients[1].Id = 2;
        patients[2].Id = 3;

        var vitals = VitalSignFaker.GenerateVitalSigns(2, 1);
        patients[0].VitalSigns = vitals;
        foreach(var vital in vitals)
        {
            vital.Patient = patients[0];
        }

        await DbContext.Patients.AddRangeAsync(patients);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _patientService.GetAllAsync(includeVitals: true);

        // Assert
        result.Should().HaveCount(3);
        result.First(p => p.Id == patients[0].Id).VitalSigns.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotReturnDeletedPatients()
    {
        // Arrange
        var patients = PatientFaker.GeneratePatients(3);
        patients[1].IsDeleted = true;

        await DbContext.Patients.AddRangeAsync(patients);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _patientService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(p => p.IsDeleted);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient_WhenPatientExists()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _patientService.GetByIdAsync(patient.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(patient.Id);
        result.Name.Should().Be(patient.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPatientDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _patientService.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatientWithVitals_WhenIncludeVitalsIsTrue()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        var vitals = VitalSignFaker.GenerateVitalSigns(3, patient.Id);

        await DbContext.Patients.AddAsync(patient);
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _patientService.GetByIdAsync(patient.Id, includeVitals: true);

        // Assert
        result.Should().NotBeNull();
        result!.VitalSigns.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnPatient()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        patient.Id = 0;

        // Act
        var result = await _patientService.CreateAsync(patient);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(patient.Name);
        result.CreatedAt.Should().Be(patient.CreatedAt);

        var savedPatient = await DbContext.Patients.FindAsync(result.Id);
        savedPatient.Should().NotBeNull();
        savedPatient!.Name.Should().Be(patient.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePatient_WhenPatientExists()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        var updatedPatient = new Patient { Id = patient.Id, Name = "Updated Name", Age = 45, RoomNumber = "101-A" };

        // Act
        var result = await _patientService.UpdateAsync(updatedPatient);

        // Assert
        result.Should().BeTrue();

        var savedPatient = await DbContext.Patients.FindAsync(patient.Id);
        savedPatient.Should().NotBeNull();
        savedPatient!.Name.Should().Be("Updated Name");
        savedPatient.Age.Should().Be(45);
        savedPatient.RoomNumber.Should().Be("101-A");
        savedPatient.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPatientDoesNotExist()
    {
        // Arrange
        var nonExistentPatient = PatientFaker.GeneratePatient();
        nonExistentPatient.Id = 999;

        // Act
        var result = await _patientService.UpdateAsync(nonExistentPatient);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePatient_WhenPatientExists()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _patientService.DeleteAsync(patient.Id);

        // Assert
        result.Should().BeTrue();

        var deletedPatient = await DbContext.Patients.FindAsync(patient.Id);
        deletedPatient.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenPatientDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _patientService.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemovePatientFromDatabase()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        // Act
        await _patientService.DeleteAsync(patient.Id);

        // Assert
        var allPatients = await _patientService.GetAllAsync();
        allPatients.Should().NotContain(p => p.Id == patient.Id);
    }
}
