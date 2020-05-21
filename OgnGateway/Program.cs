using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OgnGateway.ogn.aircraft;
using OgnGateway.ogn.config;
using OgnGateway.ogn.stream;

namespace OgnGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var configLoader = new ConfigLoader();
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

            await streamListener.Stream; //Task.Delay(10_000);
        }
    }
}