using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using WebsocketGateway.Config;
using WebsocketGateway.Hubs;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services.Publishing
{
    /// <summary>
    /// A provider that is used in the <see cref="DefaultHub"/> to give them the currently active
    /// airplane-data. We must not use the <see cref="SignalRPublishService"/> as it's an
    /// IHostedService which can not be injected by DI. But as this Provider is configured as a singleton and injected
    /// into the <see cref="SignalRPublishService"/>, we can use this as an intermediate provider.
    /// </summary>
    public class SignalRInitialDataProvider
    {
        /// <summary>
        /// The currently active flight-data.
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _activeFlightData;

        /// <summary>
        /// We need our current configuration to determine which outdated flight-data we should drop.
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        public SignalRInitialDataProvider(GatewayConfiguration gatewayConfiguration)
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
        public IReadOnlyList<FlightDataDto> GetCurrentlyActiveFlightData()
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
            var maxAgeTime = DateTime.Now.Subtract(TimeSpan.FromSeconds(_gatewayConfiguration.MaxAgeSeconds));

            _activeFlightData
                .ToList()
                .Select(entry => entry.Value)
                .Where(dto => dto.DateTime.CompareTo(maxAgeTime) == -1)
                .ForEach(dto => _activeFlightData.Remove(dto.Aircraft.Id));
        }
    }
}