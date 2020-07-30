using System.Diagnostics.CodeAnalysis;

namespace OgnGateway.ogn.models
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

        public Aircraft(string aircraftId, string? callSign = null, string? registration = null, string? type = null)
        {
            Id = aircraftId;
            CallSign = callSign;
            Registration = registration;
            Type = type;
        }

        public override string ToString()
        {
            return $"\n\t\t[Aircraft]\n\t\tID: {Id}\n\t\tcall-sign: {CallSign}\n\t\tregistration: {Registration}\n\t\ttype: {Type}";
        }
    }
}