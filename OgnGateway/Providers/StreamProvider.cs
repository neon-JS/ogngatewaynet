using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Linq;
using OgnGateway.Dtos;
using OgnGateway.Extensions.Primitives;

namespace OgnGateway.Providers
{
    /// <summary>
    /// Provider that listens to the APRS stream of the OGN servers and publishes messages to the system.
    /// </summary>
    public class StreamProvider
    {
        /// <summary>
        /// Observable representing the data stream from the OGN servers
        /// </summary>
        public IObservable<string> Stream { get; }

        /// <summary>
        /// Current configuration that is needed to connect to the OGN servers
        /// </summary>
        private readonly AprsConfig _aprsConfig;

        public StreamProvider(AprsConfig aprsConfig)
        {
            _aprsConfig = aprsConfig
                          ?? throw new ArgumentNullException(nameof(aprsConfig));
            Stream = CreateStream();
        }

        /// <summary>
        /// Creates the Observable that is listening the the OGN server
        /// </summary>
        /// <returns>Observable that is listening the the OGN server</returns>
        /// <exception cref="Exception">In case of invalid config</exception>
        private IObservable<string> CreateStream()
        {
            _aprsConfig.AprsHost.EnsureNotEmpty();
            if (_aprsConfig.AprsPort == 0)
            {
                throw new Exception("APRS port not set!");
            }

            if (
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                _aprsConfig.FilterPosition.Latitude == 0
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || _aprsConfig.FilterPosition.Longitude == 0
                || _aprsConfig.FilterRadius == 0
            )
            {
                throw new Exception("Filters not set properly!");
            }

            var latitude = _aprsConfig.FilterPosition.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = _aprsConfig.FilterPosition.Longitude.ToString(CultureInfo.InvariantCulture);
            var radius = _aprsConfig.FilterRadius;
            var loginText =
                $"user {_aprsConfig.AprsUser} pass {_aprsConfig.AprsPassword} vers ogn_gateway 1.1 filter r/{latitude}/{longitude}/{radius}";

            return Observable.Create(async (IObserver<string> observer) =>
                {
                    var client = new TcpClient();
                    await client.ConnectAsync(_aprsConfig.AprsHost, _aprsConfig.AprsPort);

                    var streamReader = new StreamReader(client.GetStream());
                    var streamWriter = new StreamWriter(client.GetStream()) {AutoFlush = true};

                    // Login on the APRS server
                    await streamWriter.WriteLineAsync(loginText);

                    // Make sure to send regular messages to keep the connection alive
                    Observable.Interval(TimeSpan.FromMinutes(10))
                        .Subscribe(async _ => await streamWriter.WriteLineAsync("# keep alive"));

                    while (true)
                    {
                        var line = await streamReader.ReadLineAsync();
                        if (line == null)
                        {
                            // Stream ended. Close the loop.
                            observer.OnCompleted();
                            break;
                        }

                        if (line.StartsWith("#") || line.Contains("TCPIP*"))
                        {
                            // Ignore server messages
                            continue;
                        }

                        observer.OnNext(line);
                    }
                })
                .Publish() // Every subscriber should subscribe to the _same_ Observable (see "hot Observable")
                .AutoConnect(); // Hot Observables must be connected. We're doing that on the first subscription.
        }
    }
}