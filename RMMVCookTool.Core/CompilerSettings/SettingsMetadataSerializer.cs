using System.Text.Json.Serialization;

namespace RMMVCookTool.Core.CompilerSettings;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(SettingsMetadata))]
public partial class SettingsMetadataSerializer : JsonSerializerContext
{
}
