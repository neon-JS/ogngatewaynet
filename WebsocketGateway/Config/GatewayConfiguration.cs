namespace WebsocketGateway.Config
{
    /// <summary>
    /// Configuration-object of the Gateway, set up by application.json
    /// </summary>
    public class GatewayConfiguration
    {
        /// <summary>
        /// If true, only events are published (Aircraft starting, aircraft landing)
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool EventsOnly { get; set; }

        /// <summary>
        /// If <see cref="EventsOnly"/> is true, this represents the minimal altitude (in m) an aircraft must reach
        /// to be considered flying. Otherwise it's seen as landed.
        /// </summary>
        public int MinimalAltitude { get; set; }

        /// <summary>
        /// If <see cref="EventsOnly"/> is true, this represents the minimal speed (in km/h) an aircraft must reach
        /// to be considered flying. Otherwise it's seen as landed.
        /// </summary>
        public int MinimalSpeed { get; set; }

        /// <summary>
        /// Number of different workers ("threads", "actors") that should handle the incoming OGN-messages.
        /// Set this number up if you have a large area with many airplanes to speed up the program.
        /// </summary>
        public int Workers { get; set; }

        /// <summary>
        /// If set, the clients only get messages all <see cref="IntervalSeconds"/> seconds.
        /// (The latest message of an aircraft is the one that will be transmitted for this specific aircraft to
        /// reduce bandwidth.)
        /// If null, all messages are passed immediately!
        /// </summary>
        public int? IntervalSeconds { get; set; }

        /// <summary>
        /// The max. age of a package that will be transmitted.
        /// All older packages will be discarded.
        /// </summary>
        public int MaxAgeSeconds { get; set; }
    }
}