using System.Diagnostics.CodeAnalysis;
using OgnGateway.ogn.models;

namespace WebsocketGateway.Models
{
    /// <summary>
    /// Representation of the data that will be sent to the clients
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class WebsocketEntry
    {
        /// <summary>
        /// Flight data as obtained by converter
        /// </summary>
        public FlightData FlightData { get; }
        
        /// <summary>
        /// Aircraft data
        /// </summary>
        public Aircraft Aircraft { get; }

        public WebsocketEntry(FlightData flightData, Aircraft aircraft)
        {
            FlightData = flightData;
            Aircraft = aircraft;
        }
    }
}