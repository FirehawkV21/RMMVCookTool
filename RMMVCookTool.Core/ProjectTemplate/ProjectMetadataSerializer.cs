using System.Text.Json.Serialization;

namespace RMMVCookTool.Core.ProjectTemplate
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(ProjectMetadata))]
    public partial class ProjectMetadataSerializer : JsonSerializerContext
    {

    }
}
