namespace RealFenixFailures.Integrations.SimConnect.Models;

public class SimAircraftState {
    public bool IsConnected { get; set; }

    // Datos de posición y movimiento
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Altitude { get; set; } // pies
    public int Heading { get; set; } // grados
    public int GroundSpeed { get; set; } // nudos
    public int AirspeedTrue { get; set; } // nudos
    public int VerticalSpeed { get; set; } // pies/minuto

    // Estado de vuelo
    public bool IsOnGround { get; set; }
    public int FlapsHandleIndex { get; set; }

    // Motores
    public bool Engine1Running { get; set; }
    public bool Engine2Running { get; set; }

    // Controles
    public double ThrottlePercent1 { get; set; }
    public double ThrottlePercent2 { get; set; }

    // Altura radio
    public int RadioHeight { get; set; }

    public SimAircraftState() {
        IsConnected = false;
    }

    // Método para clonar el estado (útil para evitar referencias compartidas)
    public SimAircraftState Clone() {
        return new SimAircraftState() {
            IsConnected = IsConnected,
            Latitude = Latitude,
            Longitude = Longitude,
            Altitude = Altitude,
            Heading = Heading,
            GroundSpeed = GroundSpeed,
            AirspeedTrue = AirspeedTrue,
            VerticalSpeed = VerticalSpeed,
            IsOnGround = IsOnGround,
            FlapsHandleIndex = FlapsHandleIndex,
            Engine1Running = Engine1Running,
            Engine2Running = Engine2Running,
            ThrottlePercent1 = ThrottlePercent1,
            ThrottlePercent2 = ThrottlePercent2,
            RadioHeight = RadioHeight
        };
    }
}