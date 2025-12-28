using System;
using System.IO;
using System.Text.Json;

namespace RelaxTimerApp
{
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VRelaxTimer",
            "settings.json"
        );

        public string Language { get; set; } = "en";
        public bool AgreementAccepted { get; set; } = false;

        private static AppSettings? _instance;
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        private static AppSettings Load()
        {
            AppSettings? settings = null;
            bool needsSave = false;

            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    settings = JsonSerializer.Deserialize<AppSettings>(json);
                }
            }
            catch
            {
                // If loading fails, we will create new settings
            }
            
            if (settings == null)
            {
                settings = new AppSettings();
                needsSave = true;
            }

            // Ensure the file exists on disk even if we just created default settings
            if (needsSave || !File.Exists(SettingsPath))
            {
                settings.Save();
            }

            return settings;
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}

