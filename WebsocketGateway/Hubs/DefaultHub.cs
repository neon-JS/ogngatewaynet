using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using WebsocketGateway.Dtos;
using WebsocketGateway.Services;

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

        public DefaultHub(LatestDataProvider latestDataProvider)
        {
            _latestDataProvider = latestDataProvider
                                          ?? throw new ArgumentNullException(nameof(latestDataProvider));
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
    }
}