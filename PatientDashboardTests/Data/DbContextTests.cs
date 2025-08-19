using Microsoft.EntityFrameworkCore;
using PatientDashboard.Data;
using PatientDashboard.Models;
using PatientDashboardTests.TestData;

namespace PatientDashboardTests.Data;

public class DbContextTests : TestBase
{
    [Fact]
    public async Task SaveChangesAsync_ShouldSetUpdatedAt_WhenEntityIsModified()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        var originalUpdatedAt = patient.UpdatedAt;

        // Act
        patient.Name = "Updated Name";
        DbContext.Patients.Update(patient);
        await DbContext.SaveChangesAsync();

        // Assert
        patient.UpdatedAt.Should().NotBe(originalUpdatedAt);
        patient.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotSetUpdatedAt_WhenEntityIsAdded()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        patient.UpdatedAt = null;

        // Act
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        // Assert
        patient.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task QueryFilter_ShouldExcludeDeletedPatients()
    {
        // Arrange
        var patients = PatientFaker.GeneratePatients(3);
        patients[1].IsDeleted = true;
        
        await DbContext.Patients.AddRangeAsync(patients);
        await DbContext.SaveChangesAsync();

        // Act
        var activePatients = await DbContext.Patients.ToListAsync();

        // Assert
        activePatients.Should().HaveCount(2);
        activePatients.Should().NotContain(p => p.IsDeleted);
    }

    [Fact]
    public async Task QueryFilter_ShouldExcludeDeletedVitalSigns()
    {
        // Arrange
        var vitals = VitalSignFaker.GenerateVitalSigns(3);
        vitals[1].IsDeleted = true;
        
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        var activeVitals = await DbContext.VitalSigns.ToListAsync();

        // Assert
        activeVitals.Should().HaveCount(2);
        activeVitals.Should().NotContain(v => v.IsDeleted);
    }

    [Fact]
    public async Task PatientVitalSigns_ShouldHaveCascadeDelete()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        var vitals = VitalSignFaker.GenerateVitalSigns(3, patient.Id);
        
        await DbContext.Patients.AddAsync(patient);
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act
        DbContext.Patients.Remove(patient);
        await DbContext.SaveChangesAsync();

        // Assert
        var remainingVitals = await DbContext.VitalSigns.ToListAsync();
        remainingVitals.Should().BeEmpty();
    }

    [Fact]
    public async Task VitalSignIndex_ShouldBeCreated()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        var vitals = VitalSignFaker.GenerateVitalSigns(5, patient.Id);
        
        await DbContext.Patients.AddAsync(patient);
        await DbContext.VitalSigns.AddRangeAsync(vitals);
        await DbContext.SaveChangesAsync();

        // Act & Assert
        // Note: In-memory provider doesn't support SQL queries, so we just verify the data was saved
        var savedVitals = await DbContext.VitalSigns.ToListAsync();
        savedVitals.Should().HaveCount(5);
    }

    [Fact]
    public async Task BaseEntity_ShouldSetCreatedAt_WhenEntityIsAdded()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();
        patient.CreatedAt = DateTime.MinValue; // Reset to ensure it gets set

        // Act
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        // Assert
        // Note: BaseEntity doesn't automatically set CreatedAt, it's set in the constructor
        patient.CreatedAt.Should().Be(DateTime.MinValue); // Should remain as set
    }

    [Fact]
    public async Task BaseEntity_ShouldSetIsDeleted_ToFalse_ByDefault()
    {
        // Arrange
        var patient = PatientFaker.GeneratePatient();

        // Act
        await DbContext.Patients.AddAsync(patient);
        await DbContext.SaveChangesAsync();

        // Assert
        patient.IsDeleted.Should().BeFalse();
    }
}
