using System.Runtime.InteropServices;

namespace RealFenixFailures.Integrations.SimConnect.Models;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct AircraftStateData {
    public double Latitude;
    public double Longitude;
    public double Altitude;
    public double Heading;
    public double GroundSpeed;
    public double AirspeedTrue;
    public double VerticalSpeed;
    public int IsOnGround;
    public double FlapsHandleIndex;
    public int Engine1Running;
    public int Engine2Running;
    public double ThrottlePercent1;
    public double ThrottlePercent2;
    public double RadioHeight;
}
