using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using OgnGateway.ogn.aircraft;
using OgnGateway.ogn.stream;
using WebsocketGateway.Config;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services
{
    public class EventOnlyDataService : DataService
    {
        private readonly ListenerConfiguration _listenerConfiguration;
        /// <summary>
        /// Contains the last FlightData of all visible aircrafts.
        /// Necessary for checking whether an aircraft has started or landed.
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _cache;
        
        public EventOnlyDataService(
            StreamListener streamListener,
            AircraftProvider aircraftProvider,
            ListenerConfiguration listenerConfiguration
            ): base(streamListener, aircraftProvider)
        {
            _listenerConfiguration = listenerConfiguration;
            _cache = new Dictionary<string, FlightDataDto>();
        }

        protected override IObservable<FlightDataDto> GetStream()
        {
            // Function that returns if the aircraft (of this FlightData) is flying or not
            var isFlying = new Func<FlightDataDto, bool>(f =>
                f.Altitude >= _listenerConfiguration.MinimalAltitude
                && f.Speed >= _listenerConfiguration.MinimalSpeed
            );
            
            // Observable that emits true when an aircraft is new, has started or landed.
            var changeEventListener = base.GetStream()
                .Where(f => !_cache.ContainsKey(f.Aircraft.Id) || isFlying(f) != isFlying(_cache[f.Aircraft.Id]))
                .Do(f => _cache[f.Aircraft.Id] = f)
                .Select(f => true);

            return base.GetStream()
                .Buffer(changeEventListener)
                .Select(flightDataList =>
                        flightDataList
                            .GroupBy(flightData => flightData.Aircraft.Id) // Make sure to not post _every_ status change for every airplane
                            .Select(group => group.First()) // Just publish the last one
                )
                .SelectMany(f => f);
        }
    }
}