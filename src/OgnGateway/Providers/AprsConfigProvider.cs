namespace OgnGateway.Providers;

/// <summary>
/// Provides the current configuration from config.json
/// </summary>
public class AprsConfigProvider
{
    private readonly string _configPath;

    public AprsConfigProvider()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                           ?? throw new NullReferenceException();
        _configPath = Path.Combine(assemblyPath, "config.json");
    }

    /// <summary>
    /// Returns the current configuration
    /// </summary>
    /// <returns>Current configuration</returns>
    public async Task<AprsConfig> LoadAsync()
    {
        return await JsonSerializer.DeserializeAsync<AprsConfig>(
            new FileStream(_configPath, FileMode.Open, FileAccess.Read)
        ) ?? throw new InvalidDataException("No valid AprsConfig found!");
    }
}