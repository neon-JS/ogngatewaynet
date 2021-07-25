using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using OgnGateway.Dtos;
using WebsocketGateway.Dtos;
using WebsocketGateway.Providers;

namespace WebsocketGateway.Hubs
{
    /// <summary>
    /// The Hub that will be used to communicate with the SignalR-clients
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DefaultHub : Hub
    {
        /// <summary>
        /// Data-provider which gives us the currently active aircraft.
        /// </summary>
        private readonly LatestDataProvider _latestDataProvider;

        /// <summary>
        /// Our current configuration
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        /// <summary>
        /// APRS-config containing the filter radius & position
        /// </summary>
        private readonly AprsConfig _aprsConfig;

        public DefaultHub(
            LatestDataProvider latestDataProvider,
            GatewayConfiguration gatewayConfiguration,
            AprsConfig aprsConfig
            )
        {
            _latestDataProvider = latestDataProvider;
            _gatewayConfiguration = gatewayConfiguration;
            _aprsConfig = aprsConfig;
        }

        /// <summary>
        /// Method that is "called" by SignalR-clients (via some SignalR-magic).
        /// Returns the currently active aircraft-information to the requesting client.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public IReadOnlyList<FlightDataDto> GetCurrentlyActiveFlightData()
        {
            return _latestDataProvider.GetLatestData();
        }

        /// <summary>
        /// Returns the current configuration of the system
        /// </summary>
        /// <returns>object</returns>
        // ReSharper disable once UnusedMember.Global
        public object GetConfiguration()
        {
            return new
            {
                _gatewayConfiguration.MaxAgeSeconds,
                _gatewayConfiguration.EventsOnly,
                _gatewayConfiguration.IntervalSeconds,
                _aprsConfig.FilterPosition,
                _aprsConfig.FilterRadius
            };
        }
    }
}
