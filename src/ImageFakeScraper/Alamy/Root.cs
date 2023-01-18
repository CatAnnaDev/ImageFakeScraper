using Newtonsoft.Json;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
namespace ImageFakeScraper.Alamy
{
    public class _450v
    {
        public string Mimetype { get; set; }
        public string Href { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class Altids
    {
        public string Id { get; set; }
        public string Seq { get; set; }
        public string Ref { get; set; }
    }

    public class Comp
    {
        public string Mimetype { get; set; }
        public string Href { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class Item
    {
        public string Uri { get; set; }
        public string Type { get; set; }
        public Altids Altids { get; set; }
        public Pseudo Pseudo { get; set; }
        public string Language { get; set; }
        public string Caption { get; set; }
        public string License { get; set; }
        public Renditions Renditions { get; set; }
        public string SamePseudoCount { get; set; }
        public string ApaLib { get; set; }
        public bool? IsNews { get; set; }
        public string SubType { get; set; }
        public DateTime? Uploaddate { get; set; }
    }

    public class Pseudo
    {
        public string Id { get; set; }
        public string Pseudono { get; set; }
        public string Name { get; set; }
    }

    public class Renditions
    {

        public Comp Comp { get; set; }
        public ZoomLarge ZoomLarge { get; set; }

        [JsonProperty("450v")]
        public _450v _450v { get; set; }
        public Thumb Thumb { get; set; }
    }

    public class Root
    {
        public string Qt { get; set; }
        public List<Item> Items { get; set; }
        public int? TotalMatches { get; set; }
        public string SearchClassification { get; set; }
        public bool? IsHardcodedClassification { get; set; }
        public int? DiscountCount { get; set; }
        public int? LibraryCount { get; set; }
        public int? CutoutCount { get; set; }
        public int? VectorCount { get; set; }
        public int? BlackAndWhiteCount { get; set; }
        public string SearchId { get; set; }
    }

    public class Thumb
    {
        public string Mimetype { get; set; }
        public string Href { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class ZoomLarge
    {
        public string Mimetype { get; set; }
        public string Href { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

}

