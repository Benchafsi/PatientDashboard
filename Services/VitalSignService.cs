using Microsoft.EntityFrameworkCore;
using PatientDashboard.Data;
using PatientDashboard.Models;
using PatientDashboard.Models.Enums;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Services;

public class VitalSignService : IVitalSignService
{
    private readonly PatientDashboardDbContext _db;
    private readonly ILogger<VitalSignService> _logger;

    public VitalSignService(PatientDashboardDbContext db, ILogger<VitalSignService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<VitalSign>> GetAllAsync(bool includePatient = false, CancellationToken ct = default)
    {
        IQueryable<VitalSign> query = _db.VitalSigns.AsNoTracking();

        if(includePatient)
            query = query.Include(v => v.Patient);

        var vitals = await query
            .OrderByDescending(v => v.MeasuredAt)
            .ToListAsync(ct);

        _logger.LogInformation("Retrieved {Count} vitals (IncludePatient={Include})", vitals.Count, includePatient);
        return vitals;
    }

    public async Task<VitalSign?> GetByIdAsync(int id, bool includePatient = false, CancellationToken ct = default)
    {
        IQueryable<VitalSign> query = _db.VitalSigns.AsNoTracking();

        if(includePatient)
            query = query.Include(v => v.Patient);

        var vital = await query.FirstOrDefaultAsync(v => v.Id == id, ct);

        if(vital is null)
            _logger.LogWarning("VitalSign with ID={Id} not found", id);
        else
            _logger.LogInformation("Retrieved VitalSign ID={Id} for PatientID={PatientId}", vital.Id, vital.PatientId);

        return vital;
    }

    public async Task<List<VitalSign>> GetByPatientIdAsync(
        int patientId,
        int? take = null,
        CancellationToken ct = default)
    {
        var query = _db.VitalSigns
            .AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.MeasuredAt);

        if(take is not null)
            query = (IOrderedQueryable<VitalSign>)query.Take(take.Value);

        var vitals = await query.ToListAsync(ct);

        _logger.LogInformation("Retrieved {Count} vitals for PatientID={PatientId}", vitals.Count, patientId);
        return vitals;
    }

    public async Task<VitalSign> CreateAsync(VitalSign vital, CancellationToken ct = default)
    {
        var patientExists = await _db.Patients.AnyAsync(p => p.Id == vital.PatientId, ct);
        if(!patientExists)
        {
            _logger.LogWarning("Create failed: patient with ID={PatientId} not found", vital.PatientId);
            throw new InvalidOperationException($"Patient with ID={vital.PatientId} not found.");
        }

        vital.Status = CalculateStatus(vital);

        _db.VitalSigns.Add(vital);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created VitalSign ID={Id} for PatientID={PatientId}", vital.Id, vital.PatientId);
        return vital;
    }

    public async Task<bool> UpdateAsync(VitalSign vital, CancellationToken ct = default)
    {
        var existing = await _db.VitalSigns.FindAsync([vital.Id], ct);
        if(existing is null)
        {
            _logger.LogWarning("Update failed: VitalSign with ID={Id} not found", vital.Id);
            return false;
        }

        if(existing.PatientId != vital.PatientId)
        {
            var targetPatientExists = await _db.Patients.AnyAsync(p => p.Id == vital.PatientId, ct);
            if(!targetPatientExists)
            {
                _logger.LogWarning("Update failed: target patient with ID={PatientId} not found", vital.PatientId);
                return false;
            }
            existing.PatientId = vital.PatientId;
        }

        existing.HeartRate = vital.HeartRate;
        existing.Systolic = vital.Systolic;
        existing.Diastolic = vital.Diastolic;
        existing.OxygenSaturation = vital.OxygenSaturation;
        existing.MeasuredAt = vital.MeasuredAt;
        existing.Status = CalculateStatus(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated VitalSign ID={Id} for PatientID={PatientId}", existing.Id, existing.PatientId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.VitalSigns.FindAsync([id], ct);
        if(existing is null)
        {
            _logger.LogWarning("Delete failed: VitalSign with ID={Id} not found", id);
            return false;
        }

        _db.VitalSigns.Remove(existing);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted VitalSign ID={Id} for PatientID={PatientId}", existing.Id, existing.PatientId);
        return true;
    }

    private static VitalStatus CalculateStatus(VitalSign v)
    {
        // Heart Rate: 60-100 N, 100-120 W, >120 C (below 60 => Warning)
        var hr = v.HeartRate switch
        {
            > 120 => VitalStatus.Critical,
            >= 100 => VitalStatus.Warning,
            < 60 => VitalStatus.Warning,
            _ => VitalStatus.Normal
        };

        // Blood Pressure: 90/60–119/79 N, 120–139/80–89 W, >139/>90 C
        var bp = (v.Systolic > 139 || v.Diastolic > 90)
            ? VitalStatus.Critical
            : (v.Systolic >= 120 || v.Diastolic >= 80)
                ? VitalStatus.Warning
                : (v.Systolic >= 90 && v.Diastolic >= 60) ? VitalStatus.Normal : VitalStatus.Warning;

        // Oxygen Saturation: >95 N, 90–95 W, <90 C
        var ox = v.OxygenSaturation < 90
            ? VitalStatus.Critical
            : v.OxygenSaturation <= 95 ? VitalStatus.Warning : VitalStatus.Normal;

        if(hr == VitalStatus.Critical || bp == VitalStatus.Critical || ox == VitalStatus.Critical)
            return VitalStatus.Critical;
        if(hr == VitalStatus.Warning || bp == VitalStatus.Warning || ox == VitalStatus.Warning)
            return VitalStatus.Warning;
        return VitalStatus.Normal;
    }
}
