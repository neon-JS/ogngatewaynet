using System;
using System.Diagnostics.CodeAnalysis;
using OgnGateway.Extensions;

namespace OgnGateway.Ogn.Models
{
    /// <summary>
    /// Representation of data that was received by the OGN live servers
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class FlightData
    {
        /// <summary>
        /// OGN-ID of the affected aircraft
        /// </summary>
        public string AircraftId { get; }

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

        public FlightData(
            string aircraftId,
            float speed,
            float altitude,
            float verticalSpeed,
            float turnRate,
            float course,
            float latitude,
            float longitude,
            DateTime dateTime
        )
        {
            aircraftId.EnsureNotEmpty();

            AircraftId = aircraftId;
            Speed = speed;
            Altitude = altitude;
            VerticalSpeed = verticalSpeed;
            TurnRate = turnRate;
            Course = course;
            DateTime = dateTime;
            Position = new Position(latitude, longitude);
        }

        public override string ToString()
        {
            return
                $"[Update]\n\tAircraft-ID: {AircraftId}\n\taltitude: {Altitude}\n\tspeed: {Speed}\n\tvertical-speed: {VerticalSpeed}"
                + $"\n\tturn-rate: {TurnRate}\n\tcourse: {Course}\n\tdatetime: {DateTime}\n\tposition: {Position}";
        }
    }
}