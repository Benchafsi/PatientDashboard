using PatientDashboard.Models.Enums;

namespace PatientDashboard.Models.Dtos;

public record VitalDto(
    int HeartRate,
    int Systolic,
    int Diastolic,
    int OxygenSaturation,
    DateTime? MeasuredAt,
    VitalStatus Status);
