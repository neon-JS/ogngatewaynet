using System;
using Akka.Actor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OgnGateway.Ogn.Config;
using OgnGateway.Ogn.Providers;
using OgnGateway.Ogn.Stream;
using WebsocketGateway.Dtos;
using WebsocketGateway.Factories;
using WebsocketGateway.Hubs;
using WebsocketGateway.Services;

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

            ConfigureConfigBasedServices(services);

            services.AddSingleton<LatestDataProvider>();
            services.AddSingleton(_ => ActorSystem.Create("WebsocketGateway"));

            services.AddSignalR();
            services.AddSingleton<ActorPropsFactory>();
            services.AddHostedService<ActorControlService>();

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
        /// Configures all services & providers that are based on the appsettings.json
        /// </summary>
        /// <param name="services">The systems' IServiceCollection</param>
        private void ConfigureConfigBasedServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var gatewayConfiguration = new GatewayConfiguration();
            Configuration.GetSection("GatewayOptions").Bind(gatewayConfiguration);
            services.AddSingleton(gatewayConfiguration);

            var aprsConfig = new AprsConfig();
            Configuration.GetSection("AprsConfig").Bind(aprsConfig);
            services.AddSingleton(aprsConfig);

            services.AddSingleton<StreamListener>();
            services.AddSingleton(_ =>
            {
                var provider = new AircraftProvider(aprsConfig);
                provider.Initialize().GetAwaiter().GetResult();
                return provider;
            });
        }
    }
}