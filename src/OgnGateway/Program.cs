using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OgnGateway.Providers;
using OgnGateway.Services;

namespace OgnGateway
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ArrangeTypeModifiers
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var configLoader = new AprsConfigProvider();
            var config = await configLoader.LoadAsync();

            var streamListener = new StreamProvider(config);
            var aircraftProvider = new AircraftProvider(config);
            var streamConverter = new StreamConverter();

            await aircraftProvider.InitializeAsync();

            streamListener.Stream
                .Subscribe(line =>
                {
                    var result = streamConverter.ConvertData(line);
                    if (result != null)
                    {
                        Console.WriteLine(result);
                        Console.WriteLine(aircraftProvider.Load(result.AircraftId));
                    }
                    else
                    {
                        Console.WriteLine($"Not parsable: {line}");
                    }
                });

            await streamListener.Stream;
        }
    }
}