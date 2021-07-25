namespace OgnGateway.Dtos
{
    /// <summary>
    /// Representation of a position (aka "coordinates")
    /// </summary>
    public record Position(float Latitude, float Longitude)
    {
        public override string ToString()
        {
            return $"\n\t\t[Position]\n\t\tLatitude: {Latitude}\n\t\tlongitude: {Longitude}";
        }
    }
}
