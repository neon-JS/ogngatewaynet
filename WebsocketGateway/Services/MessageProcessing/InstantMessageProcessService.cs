using System;
using WebsocketGateway.Config;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services.MessageProcessing
{
    /// <summary>
    /// Variant of the IMessageProcessService which publishes all incoming messages immediately.
    /// </summary>
    public class InstantMessageProcessService : AbstractMessageProcessService
    {
        public InstantMessageProcessService(GatewayConfiguration gatewayConfiguration) : base(gatewayConfiguration)
        {
        }

        /// <summary>
        /// Pushes new aircraft-data to the processing queue.
        /// </summary>
        /// <param name="data">New aircraft-data to process</param>
        public override void Push(FlightDataDto data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Publish(data);
        }
    }
}