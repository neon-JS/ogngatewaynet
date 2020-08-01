namespace WebsocketGateway.Config
{
    /// <summary>
    /// Constants that are used in the system
    /// </summary>
    public interface IConstants
    {
        /// <summary>
        /// SignalR-method that is used to provide the aircraft-data to the clients
        /// </summary>
        public const string NewDataMethod = "NewData";

        /// <summary>
        /// SignalR-method that is used by the client to obtain the currently active aircraft,
        /// so it isn't forced to wait until the next "regular" Data-message.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const string InitialRequestMethod = "GetCurrentlyActiveFlightData";
    }
}