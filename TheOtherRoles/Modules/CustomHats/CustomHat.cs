using System.Text.Json.Serialization;

namespace TheOtherRoles.Modules.CustomHats;

public class CustomHat : CustomHatHashes
{
    [JsonPropertyName("author")] public string Author { get; set; }

    [JsonPropertyName("bounce")] public bool Bounce { get; set; }

    [JsonPropertyName("climbresource")] public string ClimbResource { get; set; }

    [JsonPropertyName("condition")] public string Condition { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("package")] public string Package { get; set; }

    [JsonPropertyName("resource")] public string Resource { get; set; }

    [JsonPropertyName("adaptive")] public bool Adaptive { get; set; }

    [JsonPropertyName("behind")] public bool Behind { get; set; }

    [JsonPropertyName("backresource")] public string BackResource { get; set; }

    [JsonPropertyName("backflipresource")] public string BackFlipResource { get; set; }

    [JsonPropertyName("flipresource")] public string FlipResource { get; set; }
}