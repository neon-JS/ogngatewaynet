namespace OgnGateway.Dtos;

public record Position(
    float Latitude,
    float Longitude
)
{
    public override string ToString()
    {
        return $"Position: {{ latitude: {Latitude}, longitude: {Longitude} }}";
    }
}