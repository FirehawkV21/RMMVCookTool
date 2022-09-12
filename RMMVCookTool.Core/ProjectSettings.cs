namespace RMMVCookTool.Core;
public sealed record ProjectSettings
{
    public string FileExtension { get; set; } = "bin";
    public int CompressionLevel { get; set; } = 0;
    public bool RemoveSourceFiles { get; set; } = false;
    public bool CompressProjectFiles { get; set; } = false;
    public bool RemoveFilesAfterCompression { get; set; } = false;
}
