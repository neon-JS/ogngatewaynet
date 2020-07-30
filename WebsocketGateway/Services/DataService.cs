using System;
using System.Reactive.Linq;
using OgnGateway.ogn.aircraft;
using OgnGateway.ogn.models;
using OgnGateway.ogn.stream;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services
{
    public class DataService
    {
        public IObservable<FlightDataDto> Stream => _stream ??= GetStream();
        private IObservable<FlightDataDto>? _stream;
        private readonly StreamListener _streamListener;
        private readonly AircraftProvider _aircraftProvider;

        public DataService(
            StreamListener streamListener, 
            AircraftProvider aircraftProvider)
        {
            _streamListener = streamListener;
            _aircraftProvider = aircraftProvider;
        }

        protected virtual IObservable<FlightDataDto> GetStream()
        {
            return _stream ??= _streamListener.Stream
                .Select(StreamConverter.ConvertData)
                .OfType<FlightData>()
                .Select(data => new FlightDataDto(data, _aircraftProvider.Load(data.AircraftId)));
        }
    }
}