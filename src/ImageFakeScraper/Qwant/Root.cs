using System;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.Qwant
{
    public class Color
    {
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string selected { get; set; }
        public List<Value> values { get; set; }
    }

    public class Data
    {
        public Query query { get; set; }
        public Result result { get; set; }
    }

    public class Filters
    {
        public Size size { get; set; }
        public License license { get; set; }
        public Freshness freshness { get; set; }
        public Color color { get; set; }
        public Imagetype imagetype { get; set; }
    }

    public class Freshness
    {
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string selected { get; set; }
        public List<Value> values { get; set; }
    }

    public class Imagetype
    {
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string selected { get; set; }
        public List<Value> values { get; set; }
    }

    public class Item
    {
        public string title { get; set; }
        public string media { get; set; }
        public string thumbnail { get; set; }
        public int? thumb_width { get; set; }
        public int? thumb_height { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string url { get; set; }
        public string _id { get; set; }
        public string media_fullsize { get; set; }
        public string media_preview { get; set; }
        public string size { get; set; }
        public string thumb_type { get; set; }
    }

    public class License
    {
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string selected { get; set; }
        public List<Value> values { get; set; }
    }

    public class Query
    {
        public string locale { get; set; }
        public string query { get; set; }
        public int? offset { get; set; }
    }

    public class Result
    {
        public int? total { get; set; }
        public List<Item> items { get; set; }
        public Filters filters { get; set; }
        public bool? lastPage { get; set; }
    }

    public class Root
    {
        public string status { get; set; }
        public Data data { get; set; }
    }

    public class Size
    {
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string selected { get; set; }
        public List<Value> values { get; set; }
    }

    public class Value
    {
        public string value { get; set; }
        public string label { get; set; }
        public bool? translate { get; set; }
    }
}

