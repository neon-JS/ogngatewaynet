using System;
using OgnGateway.Dtos;

namespace WebsocketGateway.Dtos
{
    /// <summary>
    /// Representation of the data that will be sent to the clients
    /// </summary>
    public record FlightDataDto(
        float Speed,
        float Altitude,
        float VerticalSpeed,
        float TurnRate,
        float Course,
        Position Position,
        DateTime DateTime,
        AircraftDto Aircraft,
        bool Flying
    )
    {
        public override string ToString()
        {
            return
                $"[Update]\n\tAircraft-ID: {Aircraft.Id}\n\taltitude: {Altitude}\n\tspeed: {Speed}\n\tvertical-speed: {VerticalSpeed}"
                + $"\n\tturn-rate: {TurnRate}\n\tcourse: {Course}\n\tdatetime: {DateTime}\n\tposition: {Position}";
        }
    }
}
