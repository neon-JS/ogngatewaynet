using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Linq;
using OgnGateway.extensions;
using OgnGateway.ogn.config;

namespace OgnGateway.ogn.stream
{
    /// <summary>
    /// Listener that handles the connection to the OGN servers
    /// and publishes messages to the system. 
    /// </summary>
    public class StreamListener
    {
        /// <summary>
        /// Observable representing the data stream from the OGN servers
        /// </summary>
        public IObservable<string> Stream { get; }
       
        /// <summary>
        /// Current configuration that is needed to connect to the OGN servers
        /// </summary>
        private readonly Config _config;

        public StreamListener(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Stream = CreateStream();
        }
        
        /// <summary>
        /// Creates the Observable that is listening the the OGN server
        /// </summary>
        /// <returns>Observable that is listening the the OGN server</returns>
        /// <exception cref="Exception">In case of invalid config</exception>
        private IObservable<string> CreateStream()
        {
            _config.AprsHost.EnsureNotEmpty();
            if (_config.AprsPort == 0)
            {
                throw new Exception("APRS port not set!");
            }

            if (
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                _config.FilterPosition.Latitude == 0
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || _config.FilterPosition.Longitude == 0
                || _config.FilterRadius == 0
            )
            {
                throw new Exception("Filters not set properly!");
            }

            var latitude = _config.FilterPosition.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = _config.FilterPosition.Longitude.ToString(CultureInfo.InvariantCulture);
            var radius = _config.FilterRadius;
            var loginText = $"user {_config.AprsUser} pass {_config.AprsPassword} vers ogn_gateway 1.1 filter r/{latitude}/{longitude}/{radius}";

            return Observable.Create(async (IObserver<string> observer) =>
            {
                var client = new TcpClient();
                await client.ConnectAsync(_config.AprsHost, _config.AprsPort);

                var streamReader = new StreamReader(client.GetStream());
                var streamWriter = new StreamWriter(client.GetStream()) { AutoFlush = true };

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