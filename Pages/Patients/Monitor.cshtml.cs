using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientDashboard.Models.Dtos;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Pages.Patients
{
    public partial class MonitorModel : PageModel
    {
        private readonly IPatientService _patientService;
        private readonly IVitalSignService _vitalService;
        private readonly ILogger<MonitorModel> _logger;
        private readonly IVitalSimulationService _sim;

        public MonitorModel(
            IPatientService patientService,
            IVitalSignService vitalService,
            IVitalSimulationService sim,
            ILogger<MonitorModel> logger)
        {
            _patientService = patientService;
            _vitalService = vitalService;
            _logger = logger;
            _sim = sim;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public PatientDto? Patient { get; private set; }

        public List<VitalDto> Vitals { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            if(Id <= 0)
                return RedirectToPage("/Index");

            try
            {
                var patient = await _patientService.GetByIdAsync(Id, includeVitals: false, ct: ct);
                if(patient is null)
                    return NotFound();

                Patient = new PatientDto(patient.Id, patient.Name, patient.Age, patient.RoomNumber);

                var vitals = await _vitalService.GetByPatientIdAsync(Id, take: null, ct: ct);

                Vitals = vitals
                    .OrderBy(v => v.MeasuredAt)
                    .Select(v => new VitalDto(v.HeartRate, v.Systolic, v.Diastolic, v.OxygenSaturation, v.MeasuredAt))
                    .ToList();
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to load monitor data for patient {Id}", Id);
                return StatusCode(500);
            }

            return Page();
        }
    }
}
