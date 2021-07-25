using System;

namespace OgnGateway.Dtos
{
    /// <summary>
    /// Representation of data that was received by the OGN live servers
    /// </summary>
    public record FlightData(
        string AircraftId,
        float Speed,
        float Altitude,
        float VerticalSpeed,
        float TurnRate,
        float Course,
        Position Position,
        DateTime DateTime
    )
    {
        public override string ToString()
        {
            return
                $"[Update]\n\tAircraft-ID: {AircraftId}\n\taltitude: {Altitude}\n\tspeed: {Speed}\n\tvertical-speed: {VerticalSpeed}"
                + $"\n\tturn-rate: {TurnRate}\n\tcourse: {Course}\n\tdatetime: {DateTime}\n\tposition: {Position}";
        }
    }
}
