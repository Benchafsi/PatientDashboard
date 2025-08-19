using PatientDashboard.Models;
using PatientDashboard.Models.Enums;
using PatientDashboardTests.TestData;
using System.ComponentModel.DataAnnotations;

namespace PatientDashboardTests.Models;

public class VitalSignModelTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(300)]
    [InlineData(100)]
    public void VitalSign_ShouldAcceptValidHeartRate(int heartRate)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.HeartRate = heartRate;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(301)]
    public void VitalSign_ShouldRejectInvalidHeartRate(int heartRate)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.HeartRate = heartRate;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(VitalSign.HeartRate)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(300)]
    [InlineData(120)]
    public void VitalSign_ShouldAcceptValidSystolic(int systolic)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.Systolic = systolic;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(301)]
    public void VitalSign_ShouldRejectInvalidSystolic(int systolic)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.Systolic = systolic;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(VitalSign.Systolic)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200)]
    [InlineData(80)]
    public void VitalSign_ShouldAcceptValidDiastolic(int diastolic)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.Diastolic = diastolic;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(201)]
    public void VitalSign_ShouldRejectInvalidDiastolic(int diastolic)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.Diastolic = diastolic;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(VitalSign.Diastolic)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(95)]
    public void VitalSign_ShouldAcceptValidOxygenSaturation(int oxygenSaturation)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.OxygenSaturation = oxygenSaturation;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void VitalSign_ShouldRejectInvalidOxygenSaturation(int oxygenSaturation)
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();
        vital.OxygenSaturation = oxygenSaturation;

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(VitalSign.OxygenSaturation)));
    }

    [Fact]
    public void VitalSign_ShouldBeValid_WithValidData()
    {
        // Arrange
        var vital = VitalSignFaker.GenerateVitalSign();

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(vital, new ValidationContext(vital), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void VitalSign_ShouldHaveDefaultMeasuredAt()
    {
        // Arrange
        var vital = new VitalSign();

        // Act & Assert
        vital.MeasuredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void VitalSign_ShouldHaveDefaultStatus()
    {
        // Arrange
        var vital = new VitalSign();

        // Act & Assert
        vital.Status.Should().Be(VitalStatus.NA);
    }
}
