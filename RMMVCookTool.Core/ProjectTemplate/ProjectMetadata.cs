using System.Text.Json.Serialization;

namespace RMMVCookTool.Core.ProjectTemplate;

public class ProjectMetadata 
{
    [JsonPropertyName("name")]
    public string GameName { get; set; }
    [JsonPropertyName("app_name")]
    public string GameTitle { get; set; }
    [JsonPropertyName("version")]
    public string GameVersion { get; set; }
    [JsonPropertyName("main")]
    public string MainFile { get; set; }
    [JsonPropertyName("chromium-args")]
    public string ChromiumFlags { get; set; }
    [JsonPropertyName("js-flags")]
    public string JsFlags { get; set; }
    [JsonPropertyName("nodejs")]
    public bool UseNodeJs { get; set; }
    [JsonPropertyName("window")]
    public Window WindowProperties { get; set; } = new();

    public class Window
    {
        [JsonPropertyName("width")]
        public uint WindowWidth { get; set; }
        [JsonPropertyName("height")]
        public uint WindowHeight { get; set; }
        [JsonPropertyName("icon")]
        public string WindowIcon { get; set; }
        [JsonPropertyName("id")]
        public string WindowId { get; set; }
        [JsonPropertyName("min_width")]
        public uint MinimumWidth { get; set; }
        [JsonPropertyName("min_height")]
        public uint MinimumHeight { get; set; }
        [JsonPropertyName("title")]
        public string WindowTitle { get; set; }
        [JsonPropertyName("resizable")]
        public bool IsResizable { get; set; }
        [JsonPropertyName("fullscreen")]
        public bool StartAtFullScreen { get; set; }
        [JsonPropertyName("kiosk")]
        public bool RunInKioskMode { get; set; }
        [JsonPropertyName("position")]
        public string ScreenPosition { get; set; }
    }
    public ProjectMetadata()
    {
        GameName = "NewGame";
        GameTitle = "My New Game";
        JsFlags = "--expose-gc";
        ChromiumFlags = "--enable-gpu-rasterization --enable-gpu-memory-buffer-video-frames --enable-native-gpu-memory-buffers --enable-zero-copy --enable-gpu-async-worker-context";
        UseNodeJs = true;
        WindowProperties.IsResizable = true;
        WindowProperties.WindowHeight = WindowProperties.MinimumHeight = 816;
        WindowProperties.WindowWidth = WindowProperties.MinimumWidth = 624;
        WindowProperties.ScreenPosition = "none";
    }
}
