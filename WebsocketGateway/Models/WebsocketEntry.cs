using System;
using System.Diagnostics.CodeAnalysis;
using OgnGateway.ogn.models;

namespace WebsocketGateway.Models
{
    /// <summary>
    /// Representation of the data that will be sent to the clients
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class WebsocketEntry
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

        public WebsocketEntry(FlightData flightData, Aircraft aircraft)
        {
            Speed = flightData.Speed;
            Altitude = flightData.Altitude;
            VerticalSpeed = flightData.VerticalSpeed;
            TurnRate = flightData.TurnRate;
            Course = flightData.Course;
            Position = flightData.Position;
            DateTime = flightData.DateTime;
            Aircraft = aircraft;
        }
    }
}