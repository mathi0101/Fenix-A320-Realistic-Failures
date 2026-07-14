namespace RealFenixFailures.Application.DTOs;

public sealed record SimulatorAircraftState(
    bool IsConnected,

    // Posición y movimiento
    double Latitude,
    double Longitude,
    int Altitude,
    int Heading,
    int GroundSpeed,
    int AirspeedTrue,
    int VerticalSpeed,

    // Estado de vuelo
    bool IsOnGround,
    int FlapsHandleIndex,

    // Motores
    bool Engine1Running,
    bool Engine2Running,

    // Controles
    double ThrottlePercent1,
    double ThrottlePercent2,

    // Altura radio
    int RadioHeight
) {
    /// <summary>Estado desconectado por defecto. Reemplaza al constructor vacío original.</summary>
    public static SimulatorAircraftState Disconnected() => new(
        IsConnected: false,
        Latitude: 0, Longitude: 0,
        Altitude: 0, Heading: 0,
        GroundSpeed: 0, AirspeedTrue: 0,
        VerticalSpeed: 0,
        IsOnGround: false,
        FlapsHandleIndex: 0,
        Engine1Running: false,
        Engine2Running: false,
        ThrottlePercent1: 0, ThrottlePercent2: 0,
        RadioHeight: 0);
}