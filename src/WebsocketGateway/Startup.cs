using System;
using Akka.Actor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OgnGateway.Dtos;
using OgnGateway.Providers;
using WebsocketGateway.Dtos;
using WebsocketGateway.Factories;
using WebsocketGateway.Providers;
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
            services.AddSingleton<WebsocketService>();

            ConfigureConfigBasedServices(services);

            services.AddSingleton<StreamProvider>();
            services.AddSingleton<LatestDataProvider>();
            services.AddSingleton(_ => ActorSystem.Create("WebsocketGateway"));

            services.AddSingleton<ActorPropsFactory>();
            services.AddHostedService<ActorControlService>();
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(corsPolicyBuilder => 
                    corsPolicyBuilder
                        .WithOrigins("http://localhost:8000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                );
            }

            app.UseRouting();
            app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(1) });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Configures all services & providers that are based on the appsettings.json
        /// </summary>
        /// <param name="services">The systems' IServiceCollection</param>
        private void ConfigureConfigBasedServices(IServiceCollection services)
        {
            var gatewayConfiguration = new GatewayConfiguration();
            Configuration.GetSection("GatewayOptions").Bind(gatewayConfiguration);
            services.AddSingleton(gatewayConfiguration);

            var aprsConfig = new AprsConfig();
            Configuration.GetSection("AprsConfig").Bind(aprsConfig);
            services.AddSingleton(aprsConfig);

            services.AddSingleton(_ =>
            {
                var provider = new AircraftProvider(aprsConfig);
                provider.InitializeAsync().GetAwaiter().GetResult();
                return provider;
            });
        }
    }
}
