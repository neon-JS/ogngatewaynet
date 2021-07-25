using OgnGateway.Dtos;

namespace WebsocketGateway.Dtos
{
    /// <summary>
    /// Representation of an aircraft for the clients
    /// </summary>
    public record AircraftDto(string Id, string? CallSign, string? Registration, string? Type)
    {
        public AircraftDto(Aircraft aircraft) :
            this(
                aircraft.Id,
                aircraft.CallSign,
                aircraft.Registration,
                aircraft.Type
            )
        {
        }

        public override string ToString()
        {
            return
                $"\n\t\t[Aircraft]\n\t\tID: {Id}\n\t\tcall-sign: {CallSign}\n\t\tregistration: {Registration}\n\t\ttype: {Type}";
        }
    }
}
