using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PatientDashboard.Data;
using PatientDashboard.Models;
using PatientDashboard.Models.Enums;
using PatientDashboard.Services;
using PatientDashboardTests.TestData;

namespace PatientDashboardTests.Models;

public class VitalStatusCalculationTests : TestBase
{
    [Theory]
    [InlineData(60, 90, 60, 96, VitalStatus.Normal)]
    [InlineData(80, 110, 70, 98, VitalStatus.Normal)]
    [InlineData(95, 100, 65, 97, VitalStatus.Normal)]
    public void VitalSign_ShouldHaveNormalStatus_WithNormalValues(
        int heartRate, int systolic, int diastolic, int oxygenSaturation, VitalStatus expectedStatus)
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = heartRate,
            Systolic = systolic,
            Diastolic = diastolic,
            OxygenSaturation = oxygenSaturation
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(100, 120, 80, 94, VitalStatus.Warning)]
    [InlineData(110, 130, 85, 92, VitalStatus.Warning)]
    [InlineData(55, 85, 55, 95, VitalStatus.Warning)]
    [InlineData(65, 130, 85, 96, VitalStatus.Warning)]
    public void VitalSign_ShouldHaveWarningStatus_WithWarningValues(
        int heartRate, int systolic, int diastolic, int oxygenSaturation, VitalStatus expectedStatus)
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = heartRate,
            Systolic = systolic,
            Diastolic = diastolic,
            OxygenSaturation = oxygenSaturation
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(130, 150, 100, 88, VitalStatus.Critical)]
    [InlineData(140, 160, 110, 85, VitalStatus.Critical)]
    [InlineData(180, 200, 120, 82, VitalStatus.Critical)]
    [InlineData(70, 180, 95, 87, VitalStatus.Critical)]
    [InlineData(80, 120, 130, 89, VitalStatus.Critical)]
    public void VitalSign_ShouldHaveCriticalStatus_WithCriticalValues(
        int heartRate, int systolic, int diastolic, int oxygenSaturation, VitalStatus expectedStatus)
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = heartRate,
            Systolic = systolic,
            Diastolic = diastolic,
            OxygenSaturation = oxygenSaturation
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Fact]
    public void VitalSign_ShouldPrioritizeCriticalStatus_WhenMultipleVitalsAreCritical()
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = 130,      // Critical
            Systolic = 140,       // Warning
            Diastolic = 95,       // Warning
            OxygenSaturation = 88 // Critical
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        status.Should().Be(VitalStatus.Critical);
    }

    [Fact]
    public void VitalSign_ShouldPrioritizeWarningStatus_WhenMultipleVitalsAreWarning()
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = 100,      // Warning
            Systolic = 120,       // Warning
            Diastolic = 80,       // Warning
            OxygenSaturation = 96 // Normal
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        status.Should().Be(VitalStatus.Warning);
    }

    [Fact]
    public void VitalSign_ShouldReturnNormalStatus_WhenAllVitalsAreNormal()
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = 70,       // Normal
            Systolic = 110,       // Normal
            Diastolic = 70,       // Normal
            OxygenSaturation = 98 // Normal
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        status.Should().Be(VitalStatus.Normal);
    }

    [Theory]
    [InlineData(50, "Heart rate below 60 should be Warning")]
    [InlineData(59, "Heart rate below 60 should be Warning")]
    [InlineData(60, "Heart rate 60 should be Normal")]
    [InlineData(99, "Heart rate 99 should be Normal")]
    [InlineData(100, "Heart rate 100 should be Warning")]
    [InlineData(120, "Heart rate 120 should be Warning")]
    [InlineData(121, "Heart rate above 120 should be Critical")]
    [InlineData(150, "Heart rate above 120 should be Critical")]
    public void VitalSign_ShouldCalculateHeartRateStatus_Correctly(int heartRate, string description)
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = heartRate,
            Systolic = 110,       // Normal
            Diastolic = 70,       // Normal
            OxygenSaturation = 98 // Normal
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        if (heartRate < 60 || (heartRate >= 100 && heartRate <= 120))
        {
            status.Should().Be(VitalStatus.Warning, description);
        }
        else if (heartRate > 120)
        {
            status.Should().Be(VitalStatus.Critical, description);
        }
        else
        {
            status.Should().Be(VitalStatus.Normal, description);
        }
    }

    [Theory]
    [InlineData(89, 110, 70, "Oxygen saturation below 90 should be Critical")]
    [InlineData(90, 110, 70, "Oxygen saturation 90 should be Warning")]
    [InlineData(95, 110, 70, "Oxygen saturation 95 should be Warning")]
    [InlineData(96, 110, 70, "Oxygen saturation above 95 should be Normal")]
    [InlineData(100, 110, 70, "Oxygen saturation 100 should be Normal")]
    public void VitalSign_ShouldCalculateOxygenSaturationStatus_Correctly(int oxygenSaturation, int systolic, int diastolic, string description)
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = 70,       // Normal
            Systolic = systolic,
            Diastolic = diastolic,
            OxygenSaturation = oxygenSaturation
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        if (oxygenSaturation < 90)
        {
            status.Should().Be(VitalStatus.Critical, description);
        }
        else if (oxygenSaturation <= 95)
        {
            status.Should().Be(VitalStatus.Warning, description);
        }
        else
        {
            status.Should().Be(VitalStatus.Normal, description);
        }
    }

    [Theory]
    [InlineData(96, 120, 80, "Blood pressure 120/80 should be Warning")]
    [InlineData(96, 140, 90, "Blood pressure 140/90 should be Warning")]
    [InlineData(96, 150, 100, "Blood pressure above 139/90 should be Critical")]
    [InlineData(96, 110, 85, "Blood pressure 110/85 should be Warning")]
    [InlineData(96, 90, 60, "Blood pressure 90/60 should be Normal")]
    [InlineData(96, 119, 79, "Blood pressure 119/79 should be Normal")]
    public void VitalSign_ShouldCalculateBloodPressureStatus_Correctly(int oxygenSaturation, int systolic, int diastolic, string description)
    {
        // Arrange
        var vital = new VitalSign
        {
            HeartRate = 70,       // Normal
            Systolic = systolic,
            Diastolic = diastolic,
            OxygenSaturation = oxygenSaturation
        };

        // Act
        var status = VitalSignService.CalculateStatus(vital);

        // Assert
        if (systolic > 139 || diastolic > 90)
        {
            status.Should().Be(VitalStatus.Critical, description);
        }
        else if (systolic >= 120 || diastolic >= 80)
        {
            status.Should().Be(VitalStatus.Warning, description);
        }
        else if (systolic >= 90 && diastolic >= 60)
        {
            status.Should().Be(VitalStatus.Normal, description);
        }
        else
        {
            status.Should().Be(VitalStatus.Warning, description);
        }
    }
}
