using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;

namespace OgnGateway.ogn.config
{
    /// <summary>
    /// ConfigLoader that provides the current configuration from config.json
    /// </summary>
    public class ConfigLoader
    {
        /// <summary>
        /// Path of the current configuration
        /// </summary>
        private readonly string _configPath;

        public ConfigLoader()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NullReferenceException();
            _configPath = Path.Combine(assemblyPath, "config.json");
        }

        /// <summary>
        /// Returns the current configuration
        /// </summary>
        /// <returns>Current configuration</returns>
        public async Task<Config> LoadAsync()
        {
            return await JsonSerializer.DeserializeAsync<Config>(new FileStream(_configPath, FileMode.Open, FileAccess.Read));
        }
    }
}