using PatientDashboard.Models;

namespace PatientDashboard.Services.Interfaces;

public interface IVitalSignService
{
    Task<List<VitalSign>> GetAllAsync(bool includePatient = false, CancellationToken ct = default);

    Task<VitalSign?> GetByIdAsync(int id, bool includePatient = false, CancellationToken ct = default);

    Task<List<VitalSign>> GetByPatientIdAsync(int patientId, int? take = null, CancellationToken ct = default);

    Task<VitalSign> CreateAsync(VitalSign vital, CancellationToken ct = default);

    Task<bool> UpdateAsync(VitalSign vital, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}