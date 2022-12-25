using System;

namespace OgnGateway.Dtos;

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
        return $"FlightData: {{ aircraft-ID: {AircraftId}, altitude: {Altitude}, speed: {Speed}, vertical-speed: {VerticalSpeed}, turn-rate: {TurnRate}, course: {Course}, datetime: {DateTime}, position: {Position} }}";
    }
}