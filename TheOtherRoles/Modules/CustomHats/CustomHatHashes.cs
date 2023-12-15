using System.Text.Json.Serialization;

namespace TheOtherRoles.Modules.CustomHats;

public class CustomHatHashes
{
    [JsonPropertyName("reshasha")] public string ResHashA { get; set; }

    [JsonPropertyName("reshashb")] public string ResHashB { get; set; }

    [JsonPropertyName("reshashbf")] public string ResHashBf { get; set; }

    [JsonPropertyName("reshashc")] public string ResHashC { get; set; }

    [JsonPropertyName("reshashf")] public string ResHashF { get; set; }
}