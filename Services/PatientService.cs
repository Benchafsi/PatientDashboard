using Microsoft.EntityFrameworkCore;
using PatientDashboard.Data;
using PatientDashboard.Models;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Services;

public class PatientService : IPatientService
{
    private readonly PatientDashboardDbContext _db;
    private readonly ILogger<PatientService> _logger;

    public PatientService(PatientDashboardDbContext db, ILogger<PatientService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Patient>> GetAllAsync(bool includeVitals = false, CancellationToken ct = default)
    {
        IQueryable<Patient> query = _db.Patients.AsNoTracking();

        if(includeVitals)
            query = query.Include(p => p.VitalSigns);

        var patients = await query.ToListAsync(ct);

        _logger.LogInformation("Retrieved {Count} patients (IncludeVitals={Include})", patients.Count, includeVitals);
        return patients;
    }

    public async Task<Patient?> GetByIdAsync(int id, bool includeVitals = false, CancellationToken ct = default)
    {
        IQueryable<Patient> query = _db.Patients.AsNoTracking();

        if(includeVitals)
            query = query.Include(p => p.VitalSigns);

        var patient = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if(patient is null)
            _logger.LogWarning("Patient with ID={Id} not found", id);
        else
            _logger.LogInformation("Retrieved patient {Name} (ID={Id})", patient.Name, id);

        return patient;
    }

    public async Task<Patient> CreateAsync(Patient patient, CancellationToken ct = default)
    {
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created patient {Name} (ID={Id})", patient.Name, patient.Id);
        return patient;
    }

    public async Task<bool> UpdateAsync(Patient patient, CancellationToken ct = default)
    {
        var existing = await _db.Patients.FindAsync([patient.Id], ct);
        if(existing is null)
        {
            _logger.LogWarning("Update failed: patient with ID={Id} not found", patient.Id);
            return false;
        }

        existing.Name = patient.Name;
        existing.Age = patient.Age;
        existing.RoomNumber = patient.RoomNumber;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated patient {Name} (ID={Id})", existing.Name, existing.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Patients.FindAsync([id], ct);
        if(existing is null)
        {
            _logger.LogWarning("Delete failed: patient with ID={Id} not found", id);
            return false;
        }

        _db.Patients.Remove(existing);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted patient {Name} (ID={Id})", existing.Name, existing.Id);
        return true;
    }
}