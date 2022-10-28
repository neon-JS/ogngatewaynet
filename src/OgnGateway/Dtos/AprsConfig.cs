using System;

namespace OgnGateway.Dtos
{
    /// <summary>
    /// Representation of current configuration.
    /// Will automatically instantiated by the ConfigProvider
    /// </summary>
    public record AprsConfig
    {
        /// <summary>
        /// APRS host that the listener should connect to
        /// </summary>
        public string AprsHost { get; init; } = string.Empty;

        /// <summary>
        /// APRS port that the listener should connect to
        /// </summary>
        public int AprsPort { get; init; }

        /// <summary>
        /// Username that will be used while authenticating to the APRS server
        /// </summary>
        public string AprsUser { get; init; } = string.Empty;

        /// <summary>
        /// Password that will be used while authenticating to the APRS server
        /// </summary>
        public string AprsPassword { get; init; } = string.Empty;

        /// <summary>
        /// Url that contains the list of all OGN-known aircraft (OGN DDB)
        /// </summary>
        public string DdbAircraftListUrl { get; init; } = string.Empty;

        /// <summary>
        /// Position that should be listened for
        /// </summary>
        public Position FilterPosition { get; init; } = new(0, 0);

        /// <summary>
        /// Radius around the FilterPosition that should be listened for in km.
        /// </summary>
        public int FilterRadius { get; init; }
    }
}
