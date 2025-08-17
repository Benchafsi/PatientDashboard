using Microsoft.AspNetCore.SignalR;
using PatientDashboard.Models.Dtos;

namespace PatientDashboard.RealTime;

public interface IVitalSignsUpdateClient
{
    Task ReceiveVital(VitalDto vital);
}

public class VitalSignsFeedHub : Hub<IVitalSignsUpdateClient>
{
    // group name = patientId.ToString()
    public Task JoinPatient(int patientId) => Groups.AddToGroupAsync(Context.ConnectionId, patientId.ToString());

    public Task LeavePatient(int patientId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, patientId.ToString());
}
