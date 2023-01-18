using Newtonsoft.Json;
#pragma warning disable
namespace ImageFakeScraper.Pixel;

public class AdditionalData
{
    public CurrentAction? current_action { get; set; }
}

public class CurrentAction
{
    public string? external_uri { get; set; }
    public object? @params { get; set; }
}

public class Images
{
    public List<Images0>? images_0 { get; set; }
}

public class Images0
{
    [JsonProperty("`res_weight`")]
    public long? res_weight { get; set; }
    public string? basic_img_id { get; set; }
    public int? equal_count { get; set; }
    public int? has_pressfoto { get; set; }
    public int? height { get; set; }
    public string? id { get; set; }
    public string? image_page { get; set; }
    public object? is_id { get; set; }
    public string? title { get; set; }
    public string? url { get; set; }
    public int? weight { get; set; }
    public int? width { get; set; }
}

public class Root
{
    public AdditionalData? additional_data { get; set; }
    public int? agencies_count { get; set; }
    public int? count { get; set; }
    public string? distance { get; set; }
    public string? empty { get; set; }
    public string? file_path { get; set; }
    public string? hash { get; set; }
    public Images? images { get; set; }
    public int? is_guest { get; set; }
    public string? limit { get; set; }
    public string? page { get; set; }
    public Search? search { get; set; }
    public string? search_by_image { get; set; }
    public string? search_by_url { get; set; }
    public string? similarity_level { get; set; }
    public int? total_count { get; set; }
}

public class Search
{
    public string? q { get; set; }
    public string? search_content_type { get; set; }
    public int? search_time { get; set; }
    public bool? stat_buy_status { get; set; }
    public bool? stat_view_status { get; set; }
    public string? @string { get; set; }
    public string? synonyms_used { get; set; }
}
