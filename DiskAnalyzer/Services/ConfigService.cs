using System;
using System.IO;
using System.Text.Json;

namespace DiskAnalyzer.Services
{
    public class AppConfig
    {
        public string LastSelectedPath { get; set; } = "C:\\";
    }

    public class ConfigService
    {
        private readonly string _configPath;
        private AppConfig _config;

        public ConfigService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "DiskAnalyzer");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "config.json");
            Load();
        }

        public AppConfig Config => _config;

        public void Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                else
                {
                    _config = new AppConfig();
                }
            }
            catch
            {
                _config = new AppConfig();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                // Log error if needed, but don't crash
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
