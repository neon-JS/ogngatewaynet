using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using WebsocketGateway.Actors;
using WebsocketGateway.Controllers;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.DateTime;
using WebsocketGateway.Hubs;

namespace WebsocketGateway.Providers
{
    /// <summary>
    /// A provider that is used to hold active airplane-data.
    /// The messages are provided by the <see cref="PublishActor"/> and have to be
    /// stored in here to pass them to the <see cref="DefaultHub"/> or <see cref="CurrentlyActiveController"/>.
    /// </summary>
    public class LatestDataProvider
    {
        /// <summary>
        /// The currently active flight-data.
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _activeFlightData;

        /// <summary>
        /// We need our current configuration to determine which outdated flight-data we should drop.
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        public LatestDataProvider(GatewayConfiguration gatewayConfiguration)
        {
            _gatewayConfiguration = gatewayConfiguration;
            _activeFlightData = new Dictionary<string, FlightDataDto>();
        }

        /// <summary>
        /// Adds new flight-data that we should store.
        /// </summary>
        /// <param name="flightData">New flight-data</param>
        public void Push(FlightDataDto flightData)
        {
            _activeFlightData[flightData.Aircraft.Id] = flightData;
        }

        /// <summary>
        /// Returns the currently active flight-data.
        /// </summary>
        /// <returns>currently active flight-data</returns>
        public IReadOnlyList<FlightDataDto> GetLatestData()
        {
            /* Removes outdated entries. We call this method on demand as an interval-call would not make sense.*/
            _activeFlightData
                .Select(entry => entry.Value)
                .Where(dto => dto.DateTime.AddSeconds(_gatewayConfiguration.MaxAgeSeconds).IsInPast())
                .ForEach(dto => _activeFlightData.Remove(dto.Aircraft.Id));

            return _activeFlightData.Values.ToList();
        }
    }
}
