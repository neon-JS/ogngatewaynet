using System;
using System.Diagnostics.CodeAnalysis;
using OgnGateway.Dtos;

namespace WebsocketGateway.Dtos
{
    /// <summary>
    /// Representation of the data that will be sent to the clients
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FlightDataDto
    {
        /// <summary>
        /// Current speed of the aircraft in km/h
        /// </summary>
        public float Speed => _flightData.Speed;

        /// <summary>
        /// Current altitude of the aircraft in m
        /// </summary>
        public float Altitude => _flightData.Altitude;

        /// <summary>
        /// Current vertical speed of the aircraft in m/s
        /// </summary>
        public float VerticalSpeed => _flightData.VerticalSpeed;

        /// <summary>
        /// Current turn rate of the aircraft in turns/min
        /// </summary>
        public float TurnRate => _flightData.TurnRate;

        /// <summary>
        /// Current course of the aircraft
        /// </summary>
        public float Course => _flightData.Course;

        /// <summary>
        /// Current position of the aircraft
        /// </summary>
        public Position Position => _flightData.Position;

        /// <summary>
        /// DateTime of when the message was received
        /// </summary>
        public DateTime DateTime => _flightData.DateTime;

        /// <summary>
        /// Aircraft data
        /// </summary>
        public AircraftDto Aircraft { get; }

        /// <summary>
        /// Determines whether the aircraft is currently flying or not, based on the current configuration
        /// </summary>
        public bool IsFlying { get; }

        /// <summary>
        /// Flight-data to wrap
        /// </summary>
        private readonly FlightData _flightData;

        public FlightDataDto(FlightData flightData, AircraftDto aircraft, bool isFlying)
        {
            _flightData = flightData ?? throw new ArgumentNullException(nameof(flightData));
            Aircraft = aircraft ?? throw new ArgumentNullException(nameof(aircraft));
            IsFlying = isFlying;
        }

        public override string ToString()
        {
            return
                $"[Update]\n\tAircraft-ID: {Aircraft.Id}\n\taltitude: {Altitude}\n\tspeed: {Speed}\n\tvertical-speed: {VerticalSpeed}"
                + $"\n\tturn-rate: {TurnRate}\n\tcourse: {Course}\n\tdatetime: {DateTime}\n\tposition: {Position}";
        }
    }
}