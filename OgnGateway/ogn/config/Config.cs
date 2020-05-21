using System.Diagnostics.CodeAnalysis;
using OgnGateway.ogn.models;

namespace OgnGateway.ogn.config
{
    /// <summary>
    /// Representation of current configuration.
    /// Will automatically instantiated by the ConfigProvider
    /// </summary>
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Config
    {
        /// <summary>
        /// APRS host that the listener should connect to
        /// </summary>
        public string AprsHost { get; set; } = "";
        
        /// <summary>
        /// APRS port that the listener should connect to
        /// </summary>
        public int AprsPort { get; set; } = 0;
        
        /// <summary>
        /// Username that will be used while authenticating to the APRS server
        /// </summary>
        public string AprsUser { get; set; } = "";
        
        /// <summary>
        /// Password that will be used while authenticating to the APRS server
        /// </summary>
        public string AprsPassword { get; set; } = "";
        
        /// <summary>
        /// Url that contains the list of all OGN-known aircrafts
        /// </summary>
        public string AircraftListUrl { get; set; }= "";
        
        /// <summary>
        /// Position that should be listened for
        /// </summary>
        public Position FilterPosition { get; set; } = new Position();
        
        /// <summary>
        /// Radius around the FilterPosition that should be listened for in km.
        /// </summary>
        public int FilterRadius { get; set; } = 0;
    }
}