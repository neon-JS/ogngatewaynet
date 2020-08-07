using System;
using System.Diagnostics.CodeAnalysis;
using OgnGateway.Dtos;

namespace WebsocketGateway.Dtos
{
    /// <summary>
    /// Representation of an aircraft for the clients
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class AircraftDto
    {
        /// <summary>
        /// OGN-ID of the aircraft
        /// </summary>
        public string Id => _aircraft.Id;

        /// <summary>
        /// CallSign of the aircraft (e.g. "G1")
        /// </summary>
        public string? CallSign => _aircraft.CallSign;

        /// <summary>
        /// Registration of the aircraft (e.g. "D-ABCD")
        /// </summary>
        public string? Registration => _aircraft.Registration;

        /// <summary>
        /// Type of the aircraft (e.g. "Airbus A380")
        /// </summary>
        public string? Type => _aircraft.Type;

        /// <summary>
        /// The initial aircraft to wrap
        /// </summary>
        private readonly Aircraft _aircraft;

        public AircraftDto(Aircraft aircraft)
        {
            _aircraft = aircraft ?? throw new ArgumentNullException(nameof(aircraft));
        }

        public override string ToString()
        {
            return $"\n\t\t[Aircraft]\n\t\tID: {Id}\n\t\tcall-sign: {CallSign}\n\t\tregistration: {Registration}\n\t\ttype: {Type}";
        }
    }
}