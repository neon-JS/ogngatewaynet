using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OgnGateway.Ogn.Config;
using OgnGateway.Ogn.Providers;
using OgnGateway.Ogn.Stream;
using WebsocketGateway.Config;
using WebsocketGateway.Hubs;
using WebsocketGateway.Services.MessageProcessing;
using WebsocketGateway.Services.Ogn;
using WebsocketGateway.Services.Publishing;

namespace WebsocketGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure our services & providers for DI
        /// </summary>
        /// <param name="services">The systems' IServiceCollection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            ConfigureOgnGatewayServices(services);
            ConfigureConfigBasedServices(services);

            services.AddSingleton<SignalRInitialDataProvider>();
            services.AddHostedService<SignalRPublishService>();
            services.AddHostedService<OgnActorControllerService>();

            services.AddSignalR();
            services.AddCors(options => options.AddPolicy("CorsPolicyDev",
                builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins("http://localhost:8000")
                        .AllowCredentials();
                }));
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("CorsPolicyDev");
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<DefaultHub>("/websocket");
            });
        }

        /// <summary>
        /// Configures all <see cref="OgnGateway"/>-specific services
        /// </summary>
        /// <param name="services">The systems' IServiceCollection</param>
        private static void ConfigureOgnGatewayServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var config = new AprsConfigLoader().LoadAsync().GetAwaiter().GetResult();

            services.AddSingleton(new StreamListener(config));
            services.AddSingleton(_ =>
            {
                var provider = new AircraftProvider(config);
                provider.Initialize().GetAwaiter().GetResult();
                return provider;
            });
        }

        /// <summary>
        /// Configures all services & providers that are based on the appsettings.json
        /// </summary>
        /// <param name="services">The systems' IServiceCollection</param>
        private void ConfigureConfigBasedServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var gatewayConfiguration = new GatewayConfiguration();
            Configuration.GetSection("GatewayOptions").Bind(gatewayConfiguration);

            services.AddSingleton(_ => gatewayConfiguration);

            if (gatewayConfiguration.IntervalSeconds != null)
            {
                services.AddSingleton<IMessageProcessService, DelayMessageProcessService>();
            }
            else
            {
                services.AddSingleton<IMessageProcessService, InstantMessageProcessService>();
            }
        }
    }
}