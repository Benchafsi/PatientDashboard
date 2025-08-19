using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientDashboard.Models;
using PatientDashboard.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace PatientDashboard.Pages.Patients
{
    public class IndexModel : PageModel
    {
        private readonly IPatientService _patientService;
        private readonly IVitalSignService _vitalService;

        public List<Patient> Patients { get; set; } = [];

        public IndexModel(IPatientService patientService, IVitalSignService vitalService)
        {
            _patientService = patientService;
            _vitalService = vitalService;
        }

        public async Task OnGetAsync() { Patients = await _patientService.GetAllAsync(includeVitals: true); }

        public async Task<IActionResult> OnGetExportAsync(int id, CancellationToken ct)
        {
            var patient = await _patientService.GetByIdAsync(id, includeVitals: false, ct: ct);
            if(patient is null)
                return NotFound();

            var vitals = await _vitalService.GetByPatientIdAsync(id, take: null, ct: ct);

            var sb = new StringBuilder();
            sb.AppendLine("MeasuredAtUtc,HeartRate,Systolic,Diastolic,OxygenSaturation,Status");

            foreach(var v in vitals.OrderBy(v => v.MeasuredAt))
            {
                var when = v.MeasuredAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                var status = v.Status.ToString();
                sb.AppendLine($"{when},{v.HeartRate},{v.Systolic},{v.Diastolic},{v.OxygenSaturation},{status}");
            }

            var fileName = $"{SanitizeFileName(patient.Name)}_vitals_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(sb.ToString());
            return File(bytes, "text/csv", fileName);
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            return cleaned.Replace(' ', '_');
        }
    }
}
