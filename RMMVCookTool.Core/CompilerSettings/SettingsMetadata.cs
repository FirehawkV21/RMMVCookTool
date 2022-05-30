namespace RMMVCookTool.Core.CompilerSettings;

public record SettingsMetadata
{
    public string NwjsLocation { get; set; } = "";
    public ProjectSettings DefaultProjectSettings { get; set; } = new();
}
