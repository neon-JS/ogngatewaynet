using System;
using WebsocketGateway.Models;

namespace WebsocketGateway.Services.MessageProcessing
{
    /// <summary>
    /// Service that handles the filtering & processing of incoming aircraft-data (as <see cref="FlightDataDto"/>)
    /// </summary>
    public interface IMessageProcessService
    {
        /// <summary>
        /// A stream that can be observed by publishers which receives the aircraft-data (to publish)
        /// based on the given implementation. This service handles delays, events etc.
        /// </summary>
        IObservable<FlightDataDto> Stream { get; }

        /// <summary>
        /// Pushes new aircraft-data to the processing queue.
        /// </summary>
        /// <param name="data">New aircraft-data to process</param>
        void Push(FlightDataDto data);
    }
}