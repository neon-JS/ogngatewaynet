using System;
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using WebsocketGateway.Dtos;
using WebsocketGateway.Hubs;
using WebsocketGateway.Services;

namespace WebsocketGateway.Actors
{
    /// <summary>
    /// Actor which publishes incoming messages to the SignalR-clients
    /// </summary>
    public class PublishActor : ReceiveActor
    {
        /// <summary>
        /// SignalR-method that is used to provide the aircraft-data to the clients
        /// </summary>
        private const string NewDataMethod = "NewData";

        public PublishActor(
            IHubContext<DefaultHub> hubContext,
            LatestDataProvider latestDataProvider
        )
        {
            if (hubContext == null) throw new ArgumentNullException(nameof(hubContext));
            if (latestDataProvider == null) throw new ArgumentNullException(nameof(latestDataProvider));

            Receive<FlightDataDto>(async message =>
            {
                latestDataProvider.Push(message);
                await hubContext.Clients.All.SendAsync(NewDataMethod, message);
            });
        }
    }
}