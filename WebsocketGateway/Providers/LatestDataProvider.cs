using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using WebsocketGateway.Actors;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.DateTime;
using WebsocketGateway.Hubs;

namespace WebsocketGateway.Providers
{
    /// <summary>
    /// A provider that is used in the <see cref="DefaultHub"/> to give them the currently active
    /// airplane-data. The messages are provided by the <see cref="PublishActor"/> and have to be
    /// stored in here to pass them to the <see cref="DefaultHub"/>.
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
            _gatewayConfiguration = gatewayConfiguration
                                    ?? throw new ArgumentNullException(nameof(gatewayConfiguration));
            _activeFlightData = new Dictionary<string, FlightDataDto>();
        }

        /// <summary>
        /// Adds new flight-data that we should store.
        /// </summary>
        /// <param name="flightData">New flight-data</param>
        public void Push(FlightDataDto flightData)
        {
            if (flightData == null) throw new ArgumentNullException(nameof(flightData));

            _activeFlightData[flightData.Aircraft.Id] = flightData;
        }

        /// <summary>
        /// Returns the currently active flight-data.
        /// </summary>
        /// <returns>currently active flight-data</returns>
        public IReadOnlyList<FlightDataDto> GetLatestData()
        {
            CleanUpEntries();
            return _activeFlightData.Values.ToList();
        }

        /// <summary>
        /// Removes outdated entries from the <see cref="_activeFlightData"/>-list.
        /// We call this method on demand as an interval-call would not make sense.
        /// </summary>
        private void CleanUpEntries()
        {
            _activeFlightData
                .ToList()
                .Select(entry => entry.Value)
                .Where(dto => dto.DateTime.AddSeconds(_gatewayConfiguration.MaxAgeSeconds).IsInPast())
                .ForEach(dto => _activeFlightData.Remove(dto.Aircraft.Id));
        }
    }
}