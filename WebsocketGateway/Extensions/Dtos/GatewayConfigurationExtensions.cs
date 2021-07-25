using System;
using WebsocketGateway.Dtos;

namespace WebsocketGateway.Extensions.Dtos
{
    /// <summary>
    /// Class containing extension methods for the GatewayConfiguration
    /// </summary>
    public static class GatewayConfigurationExtensions
    {
        /// <summary>
        /// Returns whether the given configuration has an interval or not
        /// </summary>
        /// <param name="configuration">configuration to check</param>
        /// <returns>if configuration has an interval</returns>
        /// <exception cref="ArgumentNullException">if given configuration is null</exception>
        public static bool HasInterval(this GatewayConfiguration configuration)
        {
            return configuration.IntervalSeconds != null && configuration.IntervalSeconds != 0;
        }

        /// <summary>
        /// Returns the IntervalSeconds of a given configuration
        /// </summary>
        /// <param name="configuration">configuration of which the IntervalSeconds should be returned</param>
        /// <returns>IntervalSeconds</returns>
        /// <exception cref="ArgumentNullException">if given configuration or IntervalSeconds is null</exception>
        public static int GetIntervalSeconds(this GatewayConfiguration configuration)
        {
            return configuration.IntervalSeconds
                   ?? throw new ArgumentNullException(nameof(configuration.IntervalSeconds));
        }
    }
}
