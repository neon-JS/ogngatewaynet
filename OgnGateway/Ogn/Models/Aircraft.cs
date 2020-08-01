using System.Diagnostics.CodeAnalysis;
using OgnGateway.Extensions;

namespace OgnGateway.Ogn.Models
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Aircraft
    {
        /// <summary>
        /// OGN-ID of the aircraft
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// CallSign of the aircraft (e.g. "G1")
        /// </summary>
        public string? CallSign { get; }

        /// <summary>
        /// Registration of the aircraft (e.g. "D-ABCD")
        /// </summary>
        public string? Registration { get; }

        /// <summary>
        /// Type of the aircraft (e.g. "Airbus A380")
        /// </summary>
        public string? Type { get; }

        /// <summary>
        /// Tells whether the aircraft should be shown or not
        /// </summary>
        public bool IsVisible { get; }

        public Aircraft(string aircraftId, string? callSign, string? registration, string? type, bool isVisible)
        {
            aircraftId.EnsureNotEmpty();

            Id = aircraftId;
            CallSign = callSign;
            Registration = registration;
            Type = type;
            IsVisible = isVisible;
        }

        /// <summary>
        /// Creates an empty aircraft in case we can't find the aircraft in the <see cref="Providers.AircraftProvider"/>
        /// </summary>
        /// <param name="aircraftId">OGN-identifier of the aircraft</param>
        public Aircraft(string aircraftId)
        {
            aircraftId.EnsureNotEmpty();

            Id = aircraftId;
            CallSign = null;
            Registration = null;
            Type = null;
            IsVisible = true;
        }

        public override string ToString()
        {
            return !IsVisible
                ? $"\n\t\t[Aircraft]\n\t\tID: {Id} (Not visible)"
                : $"\n\t\t[Aircraft]\n\t\tID: {Id}\n\t\tcall-sign: {CallSign}\n\t\tregistration: {Registration}\n\t\ttype: {Type}";
        }
    }
}