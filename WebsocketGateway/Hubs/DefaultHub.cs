using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebsocketGateway.Config;
using WebsocketGateway.Services.Publishing;

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
        private readonly SignalRInitialDataProvider _signalRInitialDataProvider;

        public DefaultHub(SignalRInitialDataProvider signalRInitialDataProvider)
        {
            _signalRInitialDataProvider = signalRInitialDataProvider
                                          ?? throw new ArgumentNullException(nameof(signalRInitialDataProvider));
        }

        /// <summary>
        /// Method that is "called" by SignalR-clients (via some SignalR-magic).
        /// Returns the currently active aircraft-information to the requesting client.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public async Task InitialRequest()
        {
            await Clients.Caller.SendAsync(
                IConstants.DataMethod,
                _signalRInitialDataProvider.GetCurrentlyActiveFlightData()
            );
        }
    }
}