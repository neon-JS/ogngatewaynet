using System.Collections.Generic;
using Akka.Actor;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.DateTime;
using WebsocketGateway.Services;

namespace WebsocketGateway.Actors
{
    /// <summary>
    /// Variant of the MessageProcessActor that publishes incoming messages immediately.
    /// </summary>
    public class InstantMessageProcessActor : ReceiveActor
    {
        /// <summary>
        /// Contains the last received aircraft-data for an aircraft
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _latestAircraftMessages;

        private readonly ActorSystem _actorSystem;
        private readonly GatewayConfiguration _gatewayConfiguration;

        public InstantMessageProcessActor(
            ActorSystem actorSystem,
            GatewayConfiguration gatewayConfiguration
        )
        {
            _latestAircraftMessages = new Dictionary<string, FlightDataDto>();
            _actorSystem = actorSystem;
            _gatewayConfiguration = gatewayConfiguration;

            SetUpReceiver();
        }

        private void SetUpReceiver()
        {
            Receive<FlightDataDto>(message =>
            {
                var aircraftId = message.Aircraft.Id;

                var isEvent = !_latestAircraftMessages.ContainsKey(aircraftId)
                              || _latestAircraftMessages[aircraftId].Flying != message.Flying
                              || _latestAircraftMessages[aircraftId].DateTime
                                  .AddSeconds(_gatewayConfiguration.MaxAgeSeconds)
                                  .IsInPast();

                _latestAircraftMessages[aircraftId] = message;

                if (isEvent || !_gatewayConfiguration.EventsOnly)
                {
                    // Immediately inform the PublishActor about this message
                    _actorSystem
                        .ActorSelection($"user/{ActorControlService.PublishActorName}")
                        .Tell(message, Self);
                }
            });
        }
    }
}
