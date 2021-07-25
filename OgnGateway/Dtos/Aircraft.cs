namespace OgnGateway.Dtos
{
    /// <summary>
    /// Representation of an aircraft that published a message
    /// </summary>
    public record Aircraft(
        string Id,
        string? CallSign = null,
        string? Registration = null,
        string? Type = null,
        bool Visible = true
    )
    {
        public override string ToString()
        {
            return !Visible
                ? $"\n\t\t[Aircraft]\n\t\tID: {Id} (Not visible)"
                : $"\n\t\t[Aircraft]\n\t\tID: {Id}\n\t\tcall-sign: {CallSign}\n\t\tregistration: {Registration}\n\t\ttype: {Type}";
        }
    }
}
