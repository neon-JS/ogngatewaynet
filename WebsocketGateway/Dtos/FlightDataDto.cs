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
    public class FlightDataDto
    {
        /// <summary>
        /// Current speed of the aircraft in km/h
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Current altitude of the aircraft in m
        /// </summary>
        public float Altitude { get; }

        /// <summary>
        /// Current vertical speed of the aircraft in m/s
        /// </summary>
        public float VerticalSpeed { get; }

        /// <summary>
        /// Current turn rate of the aircraft in turns/min
        /// </summary>
        public float TurnRate { get; }

        /// <summary>
        /// Current course of the aircraft
        /// </summary>
        public float Course { get; }

        /// <summary>
        /// Current position of the aircraft
        /// </summary>
        public Position Position { get; }

        /// <summary>
        /// DateTime of when the message was received
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Aircraft data
        /// </summary>
        public Aircraft Aircraft { get; }

        /// <summary>
        /// Determines whether the aircraft is currently flying or not, based on the current configuration
        /// </summary>
        public bool IsFlying { get; }

        public FlightDataDto(FlightData flightData, Aircraft aircraft, bool isFlying)
        {
            if (flightData == null) throw new ArgumentNullException(nameof(flightData));

            Aircraft = aircraft ?? throw new ArgumentNullException(nameof(aircraft));
            Speed = flightData.Speed;
            Altitude = flightData.Altitude;
            VerticalSpeed = flightData.VerticalSpeed;
            TurnRate = flightData.TurnRate;
            Course = flightData.Course;
            Position = flightData.Position;
            DateTime = flightData.DateTime;
            IsFlying = isFlying;
        }
    }
}