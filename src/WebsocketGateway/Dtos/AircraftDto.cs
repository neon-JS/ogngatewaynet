namespace WebsocketGateway.Dtos;

public record AircraftDto(string Id, string? CallSign, string? Registration, string? Type)
{
    public AircraftDto(Aircraft aircraft) :
        this(
            aircraft.Id,
            aircraft.CallSign,
            aircraft.Registration,
            aircraft.Type
        )
    {
    }

    public override string ToString()
    {
        return $"Aircraft: {{ ID: {Id}, call-sign: {CallSign}, registration: {Registration}, type: {Type} }}";

    }
}