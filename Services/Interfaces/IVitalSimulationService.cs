namespace PatientDashboard.Services.Interfaces;

public interface IVitalSimulationService
{
    void Start(int patientId, TimeSpan duration);
}
