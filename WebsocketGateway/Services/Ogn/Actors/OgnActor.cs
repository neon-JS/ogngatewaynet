using System;
using Akka.Actor;
using OgnGateway.Ogn.Models;
using OgnGateway.Ogn.Providers;
using OgnGateway.Ogn.Stream;
using WebsocketGateway.Models;
using WebsocketGateway.Services.MessageProcessing;

namespace WebsocketGateway.Services.Ogn.Actors
{
    /// <summary>
    /// Actor which can handle the parsing-events separately.
    /// As it handles all necessary parsing-tasks, it will inform itself on the next step.
    /// </summary>
    public class OgnActor : ReceiveActor
    {
        /// <summary>
        /// Identifier of the Actor type
        /// </summary>
        public const string Name = "Main";

        public OgnActor(
            ActorSystem actorSystem,
            AircraftProvider aircraftProvider,
            IMessageProcessService messageProcessService
        )
        {
            if (actorSystem == null) throw new ArgumentNullException(nameof(actorSystem));
            if (aircraftProvider == null) throw new ArgumentNullException(nameof(aircraftProvider));
            if (messageProcessService == null) throw new ArgumentNullException(nameof(messageProcessService));

            // When receiving the raw string from the listener, convert it to FlightData and pass it to the next Actor
            Receive<string>(message =>
            {
                var convertedMessage = StreamConverter.ConvertData(message);
                if (convertedMessage == null)
                {
                    // Ignore non-parseable messages
                    return;
                }

                // The next step also happens on this Actor so tell another "Self" to handle the FlightData
                actorSystem.ActorSelection(Self.Path).Tell(convertedMessage, Self);
            });

            // When receiving FlightData, convert it into FlightDataDto and pass it to the next actor
            Receive<FlightData>(message =>
            {
                var convertedMessage = new FlightDataDto(message, aircraftProvider.Load(message.AircraftId));

                // The next step also happens on this Actor so tell another "Self" to handle the FlightData
                actorSystem.ActorSelection(Self.Path).Tell(convertedMessage, Self);
            });

            // When receiving FlightDataDto, pass it to the IMessageProcessService so it can be published.
            // Our work is finished here
            Receive<FlightDataDto>(messageProcessService.Push);
        }
    }
}