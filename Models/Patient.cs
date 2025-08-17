using System.ComponentModel.DataAnnotations;

namespace PatientDashboard.Models;

public class Patient : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [Range(0, 120)]
    public int Age { get; set; }

    [Required, MaxLength(32)]
    public string RoomNumber { get; set; } = null!;

    public List<VitalSign> VitalSigns { get; set; } = [];
}
