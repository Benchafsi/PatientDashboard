using PatientDashboard.Models;

namespace PatientDashboard.Services.Interfaces;

public interface IPatientService
{
    Task<List<Patient>> GetAllAsync(bool includeVitals = false, CancellationToken ct = default);

    Task<Patient?> GetByIdAsync(int id, bool includeVitals = false, CancellationToken ct = default);

    Task<Patient> CreateAsync(Patient patient, CancellationToken ct = default);

    Task<bool> UpdateAsync(Patient patient, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
