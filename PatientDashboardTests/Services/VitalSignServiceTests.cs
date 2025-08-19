using Microsoft.Extensions.Logging;
using Moq;
using PatientDashboard.Data;
using PatientDashboard.Models;
using PatientDashboard.Models.Enums;
using PatientDashboard.Services;
using PatientDashboardTests.TestData;

namespace PatientDashboardTests.Services;

public class VitalSignServiceTests : TestBase
{
    private readonly VitalSignService _vitalSignService;

    public VitalSignServiceTests()
    {
        _vitalSignService = new VitalSignService(DbContext, MockVitalSignLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllVitalSigns_WhenIncludePatientIsFalse()
    {
        // Arrange
        var vitals = VitalSignFaker.GenerateVitalSigns(5);
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.GetAllAsync(includePatient: false);

        // Assert
        result.Should().HaveCount(5);
        result.Should().NotContain(v => v.Patient != null);
        result.Should().BeInDescendingOrder(v => v.MeasuredAt);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnVitalSignsWithPatient_WhenIncludePatientIsTrue()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        var vitals = VitalSignFaker.GenerateVitalSigns(3, patient.Id);
        
        await DbContext.Patients.AddAsync(patient);
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.GetAllAsync(includePatient: true);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(v => v.Patient != null);
        result.Should().BeInDescendingOrder(v => v.MeasuredAt);
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotReturnDeletedVitalSigns()
    {
        // Arrange
        var vitals = VitalSignFaker.GenerateVitalSigns(3);
        vitals[1].IsDeleted = true;
        
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(v => v.IsDeleted);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnVitalSign_WhenVitalSignExists()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        await DbContext.VitalSigns.AddAsync(vital);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.GetByIdAsync(vital.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(vital.Id);
        result.HeartRate.Should().Be(vital.HeartRate);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenVitalSignDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _vitalSignService.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnVitalSignsForPatient()
    {
        // Arrange
        var patientId = 123;
        var vitals = VitalSignFaker.GenerateVitalSigns(5, patientId);
        var otherVitals = VitalSignFaker.GenerateVitalSigns(3, 456);
        
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.VitalSigns.AddRangeAsync(otherVitals);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.GetByPatientIdAsync(patientId);

        // Assert
        result.Should().HaveCount(5);
        result.Should().OnlyContain(v => v.PatientId == patientId);
        result.Should().BeInDescendingOrder(v => v.MeasuredAt);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ShouldReturnLimitedVitalSigns_WhenTakeIsSpecified()
    {
        // Arrange
        var patientId = 123;
        var vitals = VitalSignFaker.GenerateVitalSigns(10, patientId);
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.GetByPatientIdAsync(patientId, take: 3);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(v => v.MeasuredAt);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnVitalSign_WhenPatientExists()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        var vital = VitalSignFaker.GenerateVitalSign(patient.Id);
        vital.Id = 0; // Ensure new vital sign

        // Act
        var result = await _vitalSignService.CreateAsync(vital);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.PatientId.Should().Be(patient.Id);
        result.Status.Should().NotBe(VitalStatus.NA);
        
        // Verify it was saved to database
        var savedVital = await DbContext.VitalSigns.FindAsync(result.Id);
        savedVital.Should().NotBeNull();
        savedVital!.PatientId.Should().Be(patient.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenPatientDoesNotExist()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign(999); // Non-existent patient ID

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _vitalSignService.CreateAsync(vital));
    }

    [Fact]
    public async Task CreateAsync_ShouldCalculateStatusCorrectly()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        var criticalVital = VitalSignFaker.GenerateCriticalVitalSign(patient.Id);
        criticalVital.Id = 0;

        // Act
        var result = await _vitalSignService.CreateAsync(criticalVital);

        // Assert
        result.Status.Should().Be(VitalStatus.Critical);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateVitalSign_WhenVitalSignExists()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        await DbContext.VitalSigns.AddAsync(vital);
        await DbContext.SaveChangesAsync();

        var updatedVital = new VitalSign
        {
            Id = vital.Id,
            HeartRate = 85,
            Systolic = 120,
            Diastolic = 80,
            OxygenSaturation = 98,
            PatientId = vital.PatientId,
            MeasuredAt = DateTime.UtcNow
        };

        // Act
        var result = await _vitalSignService.UpdateAsync(updatedVital);

        // Assert
        result.Should().BeTrue();
        
        var savedVital = await DbContext.VitalSigns.FindAsync(vital.Id);
        savedVital.Should().NotBeNull();
        savedVital!.HeartRate.Should().Be(85);
        savedVital.Systolic.Should().Be(120);
        savedVital.Diastolic.Should().Be(80);
        savedVital.OxygenSaturation.Should().Be(98);
        savedVital.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenVitalSignDoesNotExist()
    {
        // Arrange
        var nonExistentVital = VitalSignFaker.GenerateVitalSign();
        nonExistentVital.Id = 999;

        // Act
        var result = await _vitalSignService.UpdateAsync(nonExistentVital);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldValidateNewPatientId_WhenPatientIdChanges()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        await DbContext.VitalSigns.AddAsync(vital);
        await DbContext.SaveChangesAsync();

        var newPatient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(newPatient);
        await DbContext.SaveChangesAsync();

        var updatedVital = new VitalSign
        {
            Id = vital.Id,
            HeartRate = vital.HeartRate,
            Systolic = vital.Systolic,
            Diastolic = vital.Diastolic,
            OxygenSaturation = vital.OxygenSaturation,
            PatientId = newPatient.Id,
            MeasuredAt = vital.MeasuredAt
        };

        // Act
        var result = await _vitalSignService.UpdateAsync(updatedVital);

        // Assert
        result.Should().BeTrue();
        
        var savedVital = await DbContext.VitalSigns.FindAsync(vital.Id);
        savedVital.Should().NotBeNull();
        savedVital!.PatientId.Should().Be(newPatient.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenNewPatientIdDoesNotExist()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        await DbContext.VitalSigns.AddAsync(vital);
        await DbContext.SaveChangesAsync();

        var updatedVital = new VitalSign
        {
            Id = vital.Id,
            HeartRate = vital.HeartRate,
            Systolic = vital.Systolic,
            Diastolic = vital.Diastolic,
            OxygenSaturation = vital.OxygenSaturation,
            PatientId = 999, // Non-existent patient ID
            MeasuredAt = vital.MeasuredAt
        };

        // Act
        var result = await _vitalSignService.UpdateAsync(updatedVital);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteVitalSign_WhenVitalSignExists()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        await DbContext.VitalSigns.AddAsync(vital);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _vitalSignService.DeleteAsync(vital.Id);

        // Assert
        result.Should().BeTrue();
        
        var deletedVital = await DbContext.VitalSigns.FindAsync(vital.Id);
        deletedVital.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenVitalSignDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _vitalSignService.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(60, 90, 60, 96, VitalStatus.Normal)]
    [InlineData(100, 120, 80, 94, VitalStatus.Warning)]
    [InlineData(130, 150, 100, 88, VitalStatus.Critical)]
    [InlineData(55, 85, 55, 95, VitalStatus.Warning)]
    [InlineData(80, 140, 95, 92, VitalStatus.Critical)]
    public async Task CreateAsync_ShouldCalculateCorrectStatus_ForVariousVitalValues(
        int heartRate, int systolic, int diastolic, int oxygenSaturation, VitalStatus expectedStatus)
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        var vital = new VitalSign
        {
            HeartRate = heartRate,
            Systolic = systolic,
            Diastolic = diastolic,
            OxygenSaturation = oxygenSaturation,
            PatientId = patient.Id,
            MeasuredAt = DateTime.UtcNow
        };

        // Act
        var result = await _vitalSignService.CreateAsync(vital);

        // Assert
        result.Status.Should().Be(expectedStatus);
    }
}
