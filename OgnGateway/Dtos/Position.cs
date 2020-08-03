using System.Diagnostics.CodeAnalysis;

namespace OgnGateway.Dtos
{
    /// <summary>
    /// Representation of a position (aka "coordinates")
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public class Position
    {
        /// <summary>
        /// Latitude
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public float Longitude { get; set; }

        public Position(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Position()
        {
        }

        public override string ToString()
        {
            return $"\n\t\t[Position]\n\t\tLatitude: {Latitude}\n\t\tlongitude: {Longitude}";
        }
    }
}