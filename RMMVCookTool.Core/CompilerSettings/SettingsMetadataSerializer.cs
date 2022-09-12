using System.Text.Json.Serialization;

namespace RMMVCookTool.Core.CompilerSettings;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(SettingsMetadata))]
public sealed partial class SettingsMetadataSerializer : JsonSerializerContext
{
}
