namespace ImageFakeScraper.OpenVerse;

public class Result
{
	public string? id { get; set; }
	public string? title { get; set; }
	public string? foreign_landing_url { get; set; }
	public string? url { get; set; }
	public string? creator { get; set; }
	public string? creator_url { get; set; }
	public string? license { get; set; }
	public string? license_version { get; set; }
	public string? license_url { get; set; }
	public string? provider { get; set; }
	public string? source { get; set; }
	public object? category { get; set; }
	public object? filesize { get; set; }
	public object? filetype { get; set; }
	public List<Tag>? tags { get; set; }
	public object? attribution { get; set; }
	public List<string>? fields_matched { get; set; }
	public bool mature { get; set; }
	public object? height { get; set; }
	public object? width { get; set; }
	public string? thumbnail { get; set; }
	public string? detail_url { get; set; }
	public string? related_url { get; set; }
}

public class Root
{
	public int result_count { get; set; }
	public int page_count { get; set; }
	public int page_size { get; set; }
	public int page { get; set; }
	public List<Result>? results { get; set; }
}

public class Tag
{
	public string? name { get; set; }
	public double? accuracy { get; set; }
}
