using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using WebsocketGateway.Config;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services.MessageProcessing
{
    /// <summary>
    /// Variant of the IMessageProcessService which will store & delay incoming messages for a timespan
    /// given in the current configuration. Events (aircraft started, aircraft landed) will be published
    /// immediately.
    /// </summary>
    public class DelayMessageProcessService : AbstractMessageProcessService
    {
        /// <summary>
        /// Contains all latest aircraft-data that have to be processed, identified by the aircraft-Id
        /// </summary>
        private readonly IDictionary<string, FlightDataDto> _messageQueue;

        /// <summary>
        /// Disposable stream that will periodically publish the queued messages.
        /// Must be stored to dispose it if necessary.
        /// </summary>
        private readonly IDisposable _publishStream;

        public DelayMessageProcessService(GatewayConfiguration gatewayConfiguration) : base(gatewayConfiguration)
        {
            if (gatewayConfiguration == null) throw new ArgumentNullException(nameof(gatewayConfiguration));

            _messageQueue = new Dictionary<string, FlightDataDto>();

            var delay = gatewayConfiguration.IntervalSeconds
                        ?? throw new ArgumentOutOfRangeException(nameof(gatewayConfiguration.IntervalSeconds));

            // Process all queued messages at given interval
            _publishStream = Observable
                .Interval(TimeSpan.FromSeconds(delay))
                .Subscribe(_ => ProcessQueue());
        }

        /// <summary>
        /// Pushes new aircraft-data to the processing queue.
        /// </summary>
        /// <param name="data">New aircraft-data to process</param>
        public override void Push(FlightDataDto data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _messageQueue[data.Aircraft.Id] = data;

            if (IsChangeEvent(data))
            {
                // Always publish change-events as those are the really important messages.
                Publish(data);
            }
        }

        /// <summary>
        /// Processes the queued messages, aka. it publishes them on the public stream.
        /// </summary>
        private void ProcessQueue()
        {
            _messageQueue
                .ToList()
                .ForEach(entry =>
                {
                    var (key, value) = entry;
                    Publish(value);
                    _messageQueue.Remove(key);
                });
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~DelayMessageProcessService()
        {
            _publishStream?.Dispose();
        }
    }
}