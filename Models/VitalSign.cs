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
}
