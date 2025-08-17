using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientDashboard.Models;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Pages.Patients
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPatientService _patientService;

        public List<Patient> Patients { get; set; } = [];

        public IndexModel(IPatientService patientService) { _patientService = patientService; }
        public async Task OnGetAsync() { Patients = await _patientService.GetAllAsync(includeVitals: true); }
    }
}
