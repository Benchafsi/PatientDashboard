using PatientDashboard.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientDashboard.Models;

public class VitalSign : BaseEntity
{
    [Range(0, 300)]
    public int HeartRate { get; set; }

    [Range(0, 300)]
    public int Systolic { get; set; }

    [Range(0, 200)]
    public int Diastolic { get; set; }

    [Range(0, 100)]
    public int OxygenSaturation { get; set; }

    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    public VitalStatus Status { get; set; }

    //=> CalculateStatus();

    private VitalStatus CalculateStatus()
    {
        // Heart Rate: 60-100 N, 100-120 W, >120 C (below 60 => Warning)
        var hr = HeartRate switch
        {
            > 120 => VitalStatus.Critical,
            >= 100 => VitalStatus.Warning,
            < 60 => VitalStatus.Warning,
            _ => VitalStatus.Normal
        };

        // Blood Pressure: 90/60–119/79 N, 120–139/80–89 W, >139/>90 C
        VitalStatus bp = (Systolic > 139 || Diastolic > 90)
            ? VitalStatus.Critical
            : (Systolic >= 120 || Diastolic >= 80)
                ? VitalStatus.Warning
                : (Systolic >= 90 && Diastolic >= 60) ? VitalStatus.Normal : VitalStatus.Warning;

        // Oxygen Saturation: >95 N, 90–95 W, <90 C
        VitalStatus ox = OxygenSaturation < 90
            ? VitalStatus.Critical
            : OxygenSaturation <= 95 ? VitalStatus.Warning : VitalStatus.Normal;

        // If any is Critical -> Critical; else if any Warning -> Warning; else Normal
        if(hr == VitalStatus.Critical || bp == VitalStatus.Critical || ox == VitalStatus.Critical)
            return VitalStatus.Critical;
        if(hr == VitalStatus.Warning || bp == VitalStatus.Warning || ox == VitalStatus.Warning)
            return VitalStatus.Warning;
        return VitalStatus.Normal;
    }
}
