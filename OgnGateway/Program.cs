using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OgnGateway.Ogn.Config;
using OgnGateway.Ogn.Providers;
using OgnGateway.Ogn.Stream;

namespace OgnGateway
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var configLoader = new AprsConfigLoader();
            var config = await configLoader.LoadAsync();
            
            var streamListener = new StreamListener(config);
            var aircraftProvider = new AircraftProvider(config);

            await aircraftProvider.Initialize();
            
            streamListener.Stream
                .Subscribe( line =>
                {
                    var result = StreamConverter.ConvertData(line);
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