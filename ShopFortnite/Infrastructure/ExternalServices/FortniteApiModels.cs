using System.Text.Json.Serialization;

namespace ShopFortnite.Infrastructure.ExternalServices;

// DTOs para API Fortnite
public class FortniteCosmeticsResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public List<FortniteCosmeticData> Data { get; set; } = new();
}

public class FortniteCosmeticData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public FortniteCosmeticType Type { get; set; } = new();

    [JsonPropertyName("rarity")]
    public FortniteCosmeticRarity Rarity { get; set; } = new();

    [JsonPropertyName("images")]
    public FortniteCosmeticImages Images { get; set; } = new();

    [JsonPropertyName("added")]
    public DateTime? Added { get; set; }
}

public class FortniteCosmeticType
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;
}

public class FortniteCosmeticRarity
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;
}

public class FortniteCosmeticImages
{
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("featured")]
    public string? Featured { get; set; }
}

public class FortniteShopResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public FortniteShopData Data { get; set; } = new();
}

public class FortniteShopData
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("entries")]
    public List<FortniteShopEntry> Entries { get; set; } = new();
}

public class FortniteShopEntry
{
    [JsonPropertyName("regularPrice")]
    public int RegularPrice { get; set; }

    [JsonPropertyName("finalPrice")]
    public int FinalPrice { get; set; }

    [JsonPropertyName("bundle")]
    public FortniteShopBundle? Bundle { get; set; }

    [JsonPropertyName("brItems")]
    public List<FortniteCosmeticData> BrItems { get; set; } = new();
}

public class FortniteShopBundle
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("info")]
    public string Info { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }
}
