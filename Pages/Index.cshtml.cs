using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientDashboard.Models;
using PatientDashboard.Models.Dtos;
using PatientDashboard.Models.Enums;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IPatientService _patientService;
        private readonly IVitalSignService _vitalService;

        public IndexModel(ILogger<IndexModel> logger, IPatientService patientService, IVitalSignService vitalService)
        {
            _logger = logger;
            _patientService = patientService;
            _vitalService = vitalService;
        }

        [BindProperty(SupportsGet = true)]
        public int? PatientId { get; set; }

        public List<Patient> Patients { get; private set; } = [];

        public List<VitalDto> Vitals24h { get; private set; } = [];

        public int CountNormal { get; private set; }

        public int CountWarning { get; private set; }

        public int CountCritical { get; private set; }

        public async Task OnGetAsync(CancellationToken ct)
        {
            Patients = await _patientService.GetAllAsync(includeVitals: false, ct);

            var since = DateTime.UtcNow.AddHours(-24);

            if(PatientId is > 0)
            {
                var list = await _vitalService.GetByPatientIdAsync(PatientId.Value, take: null, ct: ct);
                Vitals24h = list
                    .Where(v => v.MeasuredAt >= since)
                    .OrderBy(v => v.MeasuredAt)
                    .Select(
                        v => new VitalDto(
                            v.HeartRate,
                            v.Systolic,
                            v.Diastolic,
                            v.OxygenSaturation,
                            v.MeasuredAt,
                            v.Status))
                    .ToList();
            } else
            {
                var all = await _vitalService.GetAllAsync(includePatient: false, ct: ct);
                Vitals24h = all
                    .Where(v => v.MeasuredAt >= since)
                    .OrderBy(v => v.MeasuredAt)
                    .Select(
                        v => new VitalDto(
                            v.HeartRate,
                            v.Systolic,
                            v.Diastolic,
                            v.OxygenSaturation,
                            v.MeasuredAt,
                            v.Status))
                    .ToList();
            }

            CountNormal = Vitals24h.Count(v => v.Status == VitalStatus.Normal);
            CountWarning = Vitals24h.Count(v => v.Status == VitalStatus.Warning);
            CountCritical = Vitals24h.Count(v => v.Status == VitalStatus.Critical);
        }
    }
}
