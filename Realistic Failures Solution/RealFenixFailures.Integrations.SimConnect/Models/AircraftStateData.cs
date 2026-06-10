using System.Runtime.InteropServices;

namespace RealFenixFailures.Integrations.SimConnect.Models;

// Estructura de datos para recibir información del simulador
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct AircraftStateData {
    // Posición y movimiento
    public double Latitude;
    public double Longitude;
    public int Altitude; // pies
    public int Heading;  // grados
    public int GroundSpeed; // nudos
    public int AirspeedTrue; // nudos
    public int VerticalSpeed; // pies/minuto

    // Estado de vuelo
    public int IsOnGround;
    public double FlapsHandleIndex;

    // Motores
    public int Engine1Running;
    public int Engine2Running;

    // Controles de vuelo
    public double ThrottlePercent1;
    public double ThrottlePercent2;

    // Altura radio
    public int RadioHeight; // altura radio (pies)
}