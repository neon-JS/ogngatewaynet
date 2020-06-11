using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OgnGateway.ogn.aircraft;
using OgnGateway.ogn.config;
using OgnGateway.ogn.stream;
using WebsocketGateway.Config;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            var config = new ConfigLoader().LoadAsync().GetAwaiter().GetResult();
            
            services.AddSingleton(new StreamListener(config));
            services.AddSingleton(_ =>
            {
                var provider = new AircraftProvider(config);
                provider.Initialize().GetAwaiter().GetResult();
                return provider;
            });
            
            services.Configure<ListenerConfiguration>(Configuration.GetSection("GatewayOptions"));
            
            services.AddCors(options => options.AddPolicy("CorsPolicyDev", 
                builder => 
                {
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins("http://localhost:8000")
                        .AllowCredentials();
                }));
            
            services.AddSignalR();
            
            if (Configuration.GetSection("GatewayOptions").GetValue<bool>("EventsOnly"))
            {
                services.AddHostedService<EventOnlyPublishService>();
            }
            else
            {
                services.AddHostedService<PublishService>();
            }
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
                endpoints.MapHub<DefaultWebsocketHub>("/websocket");
            });
        }
    }
}