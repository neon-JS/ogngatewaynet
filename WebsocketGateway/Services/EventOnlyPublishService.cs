using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OgnGateway.ogn.aircraft;
using OgnGateway.ogn.models;
using OgnGateway.ogn.stream;
using WebsocketGateway.Config;
using WebsocketGateway.Hubs;

namespace WebsocketGateway.Services
{
    /// <summary>
    /// Variant of the PublishService that buffers all messages until a certain event happens.
    /// At this point, all buffered messages will be published immediately.
    /// Events:
    ///  - New aircraft occurs
    ///  - Aircraft starts
    ///  - Aircraft lands
    /// </summary>
    public class EventOnlyPublishService : PublishService
    {
        /// <summary>
        /// Representation of config (based on app settings), containing necessary information about
        /// when an aircraft is flying or not.
        /// </summary>
        private readonly ListenerConfiguration _listenerConfiguration;

        /// <summary>
        /// Contains the last FlightData of all visible aircrafts.
        /// Necessary for checking whether an aircraft has started or landed.
        /// </summary>
        private readonly IDictionary<string, FlightData> _cache;

        public EventOnlyPublishService(
            IHubContext<DefaultWebsocketHub> hubContext,
            StreamListener streamListener,
            AircraftProvider aircraftProvider,
            IOptions<ListenerConfiguration> listenerConfiguration
        ) : base(hubContext, streamListener, aircraftProvider)
        {
            _listenerConfiguration = listenerConfiguration.Value;
            _cache = new Dictionary<string, FlightData>();
        }

        protected override IObservable<FlightData> GetEventStream(IObservable<string> ognListenerStream)
        {
            // Function that returns if the aircraft (of this FlightData) is flying or not
            var isFlying = new Func<FlightData, bool>(f =>
                f.Altitude >= _listenerConfiguration.MinimalAltitude
                && f.Speed >= _listenerConfiguration.MinimalSpeed
            );
            
            // Observable that emits true when an aircraft is new, has started or landed.
            var changeEventListener = ognListenerStream
                .Select(StreamConverter.ConvertData)
                .OfType<FlightData>() // Remove nulls
                .Where(f => !_cache.ContainsKey(f.AircraftId) || isFlying(f) != isFlying(_cache[f.AircraftId]))
                .Do(f => _cache[f.AircraftId] = f)
                .Select(f => true);

            return ognListenerStream
                .Buffer(changeEventListener)
                .Select(flightDataList =>
                    flightDataList
                        .Select(StreamConverter.ConvertData) // ReSharper disable once RedundantEnumerableCastCall
                        .OfType<FlightData>()
                        .GroupBy(flightData => flightData.AircraftId) // Make sure to not post _every_ status change for every airplane
                        .Select(group => group.First()) // Just publish the last one
                )
                .SelectMany(f => f);
        }
    }
}