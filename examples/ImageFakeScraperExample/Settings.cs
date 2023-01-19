namespace ImageFakeScraperExample;
#pragma warning disable
public class Settings
{
	public Settings() { }
	public int stopAfter { get; set; }
	public bool PrintLog { get; set; }
	public bool PrintLogTag { get; set; }
}

public class Engines
{
	public Engines() { }
	public bool Google { get; set; }
	public bool UnsNapi { get; set; }
	public bool UnsNge { get; set; }
	public bool Qwant { get; set; }
	public bool Shutter { get; set; }
	public bool Open { get; set; }
	public bool Alamy { get; set; }
	public bool Bing { get; set; }
	public bool Yahoo { get; set; }
	public bool Getty { get; set; }
	public bool Immerse { get; set; }
	public bool Pixel { get; set; }
}