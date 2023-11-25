namespace WebsocketGateway.Dtos;

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
        return $"FlightData: {{ aircraft-ID: {Aircraft.Id}, altitude: {Altitude}, speed: {Speed}, vertical-speed: {VerticalSpeed}, turn-rate: {TurnRate}, course: {Course}, datetime: {DateTime}, position: {Position} }}";
    }
}