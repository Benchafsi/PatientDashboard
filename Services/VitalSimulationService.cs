using PatientDashboard.Models;
using PatientDashboard.Services.Interfaces;

namespace PatientDashboard.Services;

public class VitalSimulationService : IVitalSimulationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Random _rng = new();

    public VitalSimulationService(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

    public void Start(int patientId, TimeSpan duration)
    {
        _ = Task.Run(
            async () =>
            {
                var end = DateTime.UtcNow + duration;

                while(DateTime.UtcNow < end)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var vitalSignService = scope.ServiceProvider.GetRequiredService<IVitalSignService>();

                    // Optional: ensure patient exists; skip heavy checks for simplicity
                    var now = DateTime.UtcNow;

                    var (sys, dia) = RandomBp();
                    var vs = new VitalSign
                    {
                        PatientId = patientId,
                        MeasuredAt = now,
                        HeartRate = RandInt(60, 100),
                        OxygenSaturation = RandInt(92, 100),
                        Systolic = sys,
                        Diastolic = dia
                    };

                    await vitalSignService.CreateAsync(vs);

                    await Task.Delay(3000); // 1 Hz
                }
            });
    }

    private int RandInt(int min, int maxInclusive) => _rng.Next(min, maxInclusive + 1);

    private (int sys, int dia) RandomBp()
    {
        var sys = RandInt(100, 140);
        var dia = RandInt(60, 90);
        if(dia > sys - 20)
            dia = Math.Max(60, sys - RandInt(20, 30));
        return (sys, dia);
    }
}