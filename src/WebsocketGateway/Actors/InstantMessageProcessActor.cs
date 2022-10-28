using System.Collections.Generic;
using Akka.Actor;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.DateTime;
using WebsocketGateway.Providers;
using WebsocketGateway.Services;

namespace WebsocketGateway.Actors
{
    /// <summary>
    /// Variant of the MessageProcessActor that publishes incoming messages immediately.
    /// </summary>
    public class InstantMessageProcessActor : ReceiveActor
    {
        private readonly IActorRefFactory _actorRefFactory;
        private readonly GatewayConfiguration _gatewayConfiguration;
        private readonly ILatestDataProvider _latestDataProvider;

        public InstantMessageProcessActor(
            IActorRefFactory actorRefFactory,
            GatewayConfiguration gatewayConfiguration,
            ILatestDataProvider latestDataProvider
        )
        {
            _actorRefFactory = actorRefFactory;
            _gatewayConfiguration = gatewayConfiguration;
            _latestDataProvider = latestDataProvider;

            SetUpReceiver();
        }

        private void SetUpReceiver()
        {
            Receive<FlightDataDto>(message =>
            {
                var aircraftId = message.Aircraft.Id;
                var latestEntry = _latestDataProvider.Get(aircraftId);

                var isEvent = latestEntry is null
                              || latestEntry.Flying != message.Flying
                              || latestEntry.DateTime
                                  .AddSeconds(_gatewayConfiguration.MaxAgeSeconds)
                                  .IsInPast();

                if (isEvent || !_gatewayConfiguration.EventsOnly)
                {
                    // Immediately inform the PublishActor about this message
                    _actorRefFactory
                        .ActorSelection($"user/{ActorControlHostedService.PublishActorName}")
                        .Tell(message, Self);
                }
            });
        }
    }
}
