namespace OgnGateway.Dtos
{
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
                ? $"Aircraft: {{ <INVISIBLE> }}"
                : $"Aircraft: {{ ID: {Id}, call-sign: {CallSign}, registration: {Registration}, type: {Type} }}";
        }
    }
}
