namespace PatientDashboard.Pages.Patients
{
    public partial class MonitorModel
    {
        public record PatientDto(int Id, string Name, int Age, string RoomNumber);
    }
}
