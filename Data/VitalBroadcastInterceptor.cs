using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PatientDashboard.Models;
using PatientDashboard.Models.Dtos;
using PatientDashboard.RealTime;

namespace PatientDashboard.Data;

public class VitalBroadcastInterceptor : SaveChangesInterceptor
{
    private readonly IHubContext<VitalSignsFeedHub, IVitalSignsUpdateClient> _hub;

    public VitalBroadcastInterceptor(IHubContext<VitalSignsFeedHub, IVitalSignsUpdateClient> hub) { _hub = hub; }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    { return await base.SavingChangesAsync(eventData, result, cancellationToken); }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if(ctx is null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);

        // Grab entries added in this SaveChanges after save they’re Unchanged thats why we check for Added or Unchanged state.
        var addedVitals = ctx.ChangeTracker
            .Entries<VitalSign>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Unchanged)
            .Select(e => e.Entity)
            .ToList();

        foreach(var v in addedVitals)
        {
            await _hub.Clients
                .Group(v.PatientId.ToString())
                .ReceiveVital(new VitalDto(v.HeartRate, v.Systolic, v.Diastolic, v.OxygenSaturation, v.MeasuredAt));
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}