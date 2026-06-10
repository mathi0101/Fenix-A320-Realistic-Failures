using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Integrations.SimConnect.Models;

public class SimAircraftContext {
    public FlightPhaseEnum FlightPhase { get; set; }
    public double Altitude { get; set; } // pies
    public double Airspeed { get; set; } // nudos
    public double VerticalSpeed { get; set; } // pies/minuto
    public bool IsOnGround { get; set; }
    public double Heading { get; set; } // grados
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadioHeight { get; set; } // pies
    public int EnginesRunning { get; set; } // número de motores en marcha
}