using PatientDashboard.Models;
using PatientDashboard.Models.Enums;
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

                    var now = DateTime.UtcNow;
                    var status = WeightedStatus();                 // 60% N, 30% W, 10% C
                    var vs = MakeVitalForStatus(patientId, now, status);

                    await vitalSignService.CreateAsync(vs);

                    await Task.Delay(3000); // ~0.33 Hz
                }
            });
    }

    // --- helpers for distribution & ranges ---

    private VitalStatus WeightedStatus()
    {
        // 0.0-0.6 -> Normal, 0.6-0.9 -> Warning, 0.9-1.0 -> Critical
        var r = _rng.NextDouble();
        if(r < 0.60)
            return VitalStatus.Normal;
        if(r < 0.90)
            return VitalStatus.Warning;
        return VitalStatus.Critical;
    }

    private VitalSign MakeVitalForStatus(int patientId, DateTime measuredAt, VitalStatus status)
    {
        int hr, spo2, sys, dia;

        switch(status)
        {
            case VitalStatus.Normal:
                hr = Rand(60, 100);
                spo2 = Rand(96, 100);
                (sys, dia) = BpNormal();
                break;

            case VitalStatus.Warning:
                var wPick = Rand(0, 2);
                if(wPick == 0)
                {
                    hr = Rand(105, 120);
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpNormal();
                } else if(wPick == 1)
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(90, 95);
                    (sys, dia) = BpNormal();
                } else
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpWarning();
                }
                break;

            case VitalStatus.Critical:
            default:
                var cPick = Rand(0, 2);
                if(cPick == 0)
                {
                    hr = Rand(130, 160);
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpNormal();
                } else if(cPick == 1)
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(85, 89);
                    (sys, dia) = BpNormal();
                } else
                {
                    hr = Rand(60, 100);
                    spo2 = Rand(96, 100);
                    (sys, dia) = BpCritical();
                }
                break;
        }

        return new VitalSign
        {
            PatientId = patientId,
            MeasuredAt = measuredAt,
            HeartRate = hr,
            OxygenSaturation = spo2,
            Systolic = sys,
            Diastolic = dia
        };
    }

    private (int sys, int dia) BpNormal()
    {
        var sys = Rand(95, 119);
        var dia = Rand(60, 79);
        dia = Math.Min(dia, sys - 20);
        return (sys, Math.Max(50, dia));
    }

    private (int sys, int dia) BpWarning()
    {
        var choose = Rand(0, 1);
        int sys, dia;
        if(choose == 0)
        {
            sys = Rand(120, 139);
            dia = Rand(70, 89);
        } else
        {
            sys = Rand(110, 139);
            dia = Rand(80, 89);
        }
        dia = Math.Min(dia, sys - 20);
        return (sys, Math.Max(50, dia));
    }

    private (int sys, int dia) BpCritical()
    {
        var choose = Rand(0, 1);
        int sys, dia;
        if(choose == 0)
        {
            sys = Rand(140, 180);
            dia = Rand(80, 100);
        } else
        {
            sys = Rand(130, 170);
            dia = Rand(91, 110);
        }
        dia = Math.Min(dia, sys - 20);
        return (sys, Math.Max(50, dia));
    }

    private int Rand(int min, int maxInclusive) => _rng.Next(min, maxInclusive + 1);
}