using System.Text.Json;

namespace RMMVCookTool.Core.CompilerSettings;

public sealed class CompilerSettingsManager
{
    public SettingsMetadata Settings { get; set; }
    private static string SettingsFolderLocation { get {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RMMVCookTool", "Settings");
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "RMMVCookTool", "Settings");
            }
        } }

    private static string SettingsFileLocation => Path.Combine(SettingsFolderLocation, "CompilerSettings.json");        
    public CompilerSettingsManager()
    {
        Settings = new();
        InitializeSettings();
    }
    public void LoadSettings()
    {
        if (File.Exists(SettingsFileLocation))
        {
            string inputFile = File.ReadAllText(SettingsFileLocation);
            Settings = JsonSerializer.Deserialize(inputFile, SettingsMetadataSerializer.Default.SettingsMetadata);
        }
    }
    public void SaveSettings()
    {
        string output = JsonSerializer.Serialize(Settings, SettingsMetadataSerializer.Default.SettingsMetadata);
        File.WriteAllText(SettingsFileLocation, output);
    }

    public void InitializeSettings()
    {
        if (File.Exists(SettingsFileLocation)) LoadSettings();
        else
        {
            if (!Directory.Exists(SettingsFolderLocation)) Directory.CreateDirectory(SettingsFolderLocation);
            SaveSettings();
        }
    }
}
