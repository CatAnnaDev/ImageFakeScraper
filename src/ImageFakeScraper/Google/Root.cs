using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.Google
{
	public class Gsa
	{
		public string file_size { get; set; }
		public string referrer_host { get; set; }
	}

	public class Ischj
	{
		public List<Metadata> metadata { get; set; }
	}

	public class Layout
	{
		public bool? strict_cropping { get; set; }
		public int? crop_bottom { get; set; }
		public int? crop_left { get; set; }
		public int? crop_right { get; set; }
		public int? crop_top { get; set; }
	}

	public class Metadata
	{
		public string background_color { get; set; }
		public Gsa gsa { get; set; }
		public bool? is_animated { get; set; }
		public Layout layout { get; set; }
		public OriginalImage original_image { get; set; }
		public Result result { get; set; }
		public string result_type { get; set; }
		public string rich_metadata_type { get; set; }
		public TextInGrid text_in_grid { get; set; }
		public Thumbnail thumbnail { get; set; }
		public string unique_id { get; set; }
		public string ved { get; set; }
		public RichMetadata rich_metadata { get; set; }
	}

	public class OriginalImage
	{
		public int? height { get; set; }
		public string url { get; set; }
		public int? width { get; set; }
	}

	public class Product
	{
		public bool? availability { get; set; }
		public string brand { get; set; }
		public string description { get; set; }
		public string name { get; set; }
		public string price_currency { get; set; }
		public double? price_value { get; set; }
	}

	public class RenderedResultPreviewSize
	{
		public string max_thumbnail_shown { get; set; }
	}

	public class Result
	{
		public bool? collected { get; set; }
		public string image_source_url { get; set; }
		public bool? is_from_teragoogle { get; set; }
		public bool? is_from_visual_dictionary { get; set; }
		public bool? limit_display_size { get; set; }
		public string page_title { get; set; }
		public string referrer_doc_id { get; set; }
		public string referrer_url { get; set; }
		public RenderedResultPreviewSize rendered_result_preview_size { get; set; }
		public string site_title { get; set; }
		public bool? disable_hotlink { get; set; }
		public string fallback_proxy_url { get; set; }
	}

	public class RichMetadata
	{
		public Product product { get; set; }
	}

	public class Root
	{
		public Ischj ischj { get; set; }
	}

	public class TextInGrid
	{
		public string snippet { get; set; }
	}

	public class Thumbnail
	{
		public int? height { get; set; }
		public string url { get; set; }
		public int? width { get; set; }
	}
}
