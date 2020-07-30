using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using WebsocketGateway.Config;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services.MessageProcessing
{
    /// <summary>
    /// Abstract variant of IMessageProcessService that contains some shared logic (e.g. check if given data
    /// represents an event) and makes sure to only publish messages that are allowed by current configuration.
    /// </summary>
    public abstract class AbstractMessageProcessService : IMessageProcessService
    {
        /// <summary>
        /// A stream that can be observed by publishers which receives the aircraft-data (to publish)
        /// based on the given implementation. This service handles delays, events etc.
        /// </summary>
        public IObservable<FlightDataDto> Stream => _subject;

        /// <summary>
        /// Current configuration, used to determine whether an aircraft is flying or not.
        /// </summary>
        private readonly GatewayConfiguration _gatewayConfiguration;

        /// <summary>
        /// Contains the flying-state (aka. "Is the aircraft currently flying?") for the last processed message
        /// </summary>
        private readonly IDictionary<string, bool> _flyingCache;

        /// <summary>
        /// A stream that can be observed by publishers which receives the aircraft-data (to publish)
        /// based on the given implementation. This service handles delays, events etc.
        /// </summary>
        private readonly ISubject<FlightDataDto> _subject;

        protected AbstractMessageProcessService(GatewayConfiguration gatewayConfiguration)
        {
            _gatewayConfiguration = gatewayConfiguration
                                    ?? throw new ArgumentNullException(nameof(gatewayConfiguration));
            _flyingCache = new Dictionary<string, bool>();
            _subject = new Subject<FlightDataDto>();
        }

        /// <summary>
        /// Pushes new aircraft-data to the processing queue.
        /// </summary>
        /// <param name="data">New aircraft-data to process</param>
        public abstract void Push(FlightDataDto data);

        /// <summary>
        /// Returns whether the given data represents an event (aircraft started, aircraft landed, new aircraft).
        /// </summary>
        /// <param name="data">The <see cref="FlightDataDto"/> to check</param>
        /// <returns>true if the given data represents an event</returns>
        protected bool IsChangeEvent(FlightDataDto data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var aircraftId = data.Aircraft.Id;

            var isNowFlying = data.Altitude >= _gatewayConfiguration.MinimalAltitude
                              && data.Speed >= _gatewayConfiguration.MinimalSpeed;

            var hasChanged = !_flyingCache.ContainsKey(aircraftId)
                             || isNowFlying != _flyingCache[aircraftId];

            if (hasChanged)
            {
                _flyingCache[aircraftId] = isNowFlying;
            }

            return hasChanged;
        }

        /// <summary>
        /// Takes the data that a concrete implementation wants to publish and
        /// publishes it on the stream if the current configuration allows this.
        /// </summary>
        /// <param name="data">The data that should be published</param>
        protected void Publish(FlightDataDto data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (!IsChangeEvent(data) && _gatewayConfiguration.EventsOnly)
            {
                return;
            }

            _subject.OnNext(data);
        }
    }
}