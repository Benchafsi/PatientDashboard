using Microsoft.AspNetCore.Mvc;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IVitalSignService _vitalSignService;
    private readonly IVitalSimulationService _sim;

    public PatientsController(
        IPatientService patientService,
        IVitalSignService vitalSignService,
        IVitalSimulationService sim)
    {
        _patientService = patientService;
        _vitalSignService = vitalSignService;
        _sim = sim;
    }

    // GET /api/patients
    [HttpGet]
    public async Task<IActionResult> GetPatients() => Ok(await _patientService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var p = await _patientService.GetByIdAsync(id);

        return p is null ? NotFound() : Ok(p);
    }

    // GET /api/patients/{id}/vitals (for charts & dashboard)
    [HttpGet("{id:int}/vitals")]
    public async Task<IActionResult> GetVitals(int id)
    {
        var vitals = await _vitalSignService.GetByPatientIdAsync(id);

        return vitals is { Count :> 0 } ? Ok(vitals) : NotFound();
    }

    [HttpPost("{id:int}/vitals")]
    public async Task<IActionResult> StartSimulation(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if(patient is null)
            return NotFound();

        _sim.Start(id, TimeSpan.FromSeconds(30));
        return Accepted(new { started = true, id });
    }
}