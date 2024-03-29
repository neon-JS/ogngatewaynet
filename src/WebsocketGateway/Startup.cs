namespace WebsocketGateway;

public class Startup(IConfiguration configuration)
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IConfiguration Configuration { get; } = configuration;

    /// <summary>
    /// Configure our services & providers for DI
    /// </summary>
    /// <param name="services">The systems' IServiceCollection</param>
    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureConfigBasedServices(services);
        ConfigureActorSystem(services);

        services.AddSingleton<IStreamProvider, StreamProvider>();
        services.AddSingleton<IStreamConverter, StreamConverter>();

        services.AddSingleton<IActorPropsFactory, ActorPropsFactory>();
        services.AddSingleton<ILatestDataProvider, LatestDataProvider>();
        services.AddSingleton<IWebsocketService, WebsocketService>();

        services.AddHostedService<ActorControlHostedService>();

        services.AddControllers();
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
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    private void ConfigureActorSystem(IServiceCollection services)
    {
        var actorSystem = ActorSystem.Create("WebsocketGateway");

        services.AddSingleton(actorSystem);
        services.AddSingleton<IActorRefFactory>(actorSystem);
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

        services.AddSingleton<IAircraftProvider>(_ =>
        {
            var provider = new AircraftProvider(aprsConfig);
            provider.InitializeAsync().GetAwaiter().GetResult();
            return provider;
        });
    }
}