namespace RMMVCookTool.Core.CompilerSettings;

public record SettingsMetadata
{
    public string NwjsLocation { get; set; } = "";
    public ProjectSettings DefaultProjectSettings { get; set; } = new();
}

public record ProjectSettings
{
    public string FileExtension { get; set; } = ".bin";
    public int CompressionLevel { get; set; } = 2;
    public bool RemoveSourceFiles { get; set; } = false;
    public bool CompressProjectFiles { get; set; } = false;
    public bool RemoveFilesAfterCompression { get; set; } = false;
}
