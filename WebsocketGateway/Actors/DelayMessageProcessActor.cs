using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using WebsocketGateway.Dtos;
using WebsocketGateway.Extensions.DateTime;
using WebsocketGateway.Services;

namespace WebsocketGateway.Actors
{
    /// <summary>
    /// Variant of the MessageProcessActor that queues and delays incoming messages for the configured timespan.
    /// Event messages are published immediately.
    /// </summary>
    public class DelayMessageProcessActor : ReceiveActor
    {
        /// <summary>
        /// When received, the DelayMessageProcessActor will push all its messages to the PublishActor
        /// </summary>
        public class PushMessages
        {
        }

        /// <summary>
        /// Contains all latest aircraft-data that have to be processed, identified by the aircraft-Id
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _messageQueue;

        /// <summary>
        /// Contains the last received aircraft-data for an aircraft
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _latestAircraftMessages;

        /// <summary>
        /// Akka.NET ActorSystem that is needed to find the next Actor if necessary
        /// </summary>
        private readonly ActorSystem _actorSystem;

        /// <summary>
        /// Our current configuration
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        public DelayMessageProcessActor(
            ActorSystem actorSystem,
            GatewayConfiguration gatewayConfiguration
        )
        {
            _actorSystem = actorSystem
                           ?? throw new ArgumentNullException(nameof(actorSystem));
            _gatewayConfiguration = gatewayConfiguration
                                    ?? throw new ArgumentNullException(nameof(gatewayConfiguration));
            _latestAircraftMessages = new Dictionary<string, FlightDataDto>();
            _messageQueue = new Dictionary<string, FlightDataDto>();

            SetUpReceiver();
        }

        /// <summary>
        /// Sets up the Actor so it handles incoming messages
        /// </summary>
        private void SetUpReceiver()
        {
            Receive<FlightDataDto>(message =>
            {
                var aircraftId = message.Aircraft.Id;

                var isEvent = !_latestAircraftMessages.ContainsKey(aircraftId)
                              || _latestAircraftMessages[aircraftId].IsFlying != message.IsFlying
                              || _latestAircraftMessages[aircraftId].DateTime
                                  .AddSeconds(_gatewayConfiguration.MaxAgeSeconds)
                                  .IsInPast();

                _latestAircraftMessages[aircraftId] = message;

                if (isEvent)
                {
                    // Immediately inform the PublishActor about this message as it's an event
                    _actorSystem
                        .ActorSelection($"user/{ActorControlService.PublishActorName}")
                        .Tell(message, Self);
                }
                else if (!_gatewayConfiguration.EventsOnly)
                {
                    // Just queue the message so it can be handled later
                    _messageQueue[aircraftId] = message;
                }
            });

            Receive<PushMessages>(_ =>
            {
                var entries = _messageQueue.ToList();
                _messageQueue.Clear();

                entries
                    .Select(kv => kv.Value)
                    .ForEach(entry =>
                        _actorSystem
                            .ActorSelection($"user/{ActorControlService.PublishActorName}")
                            .Tell(entry, Self)
                    );
            });
        }
    }
}