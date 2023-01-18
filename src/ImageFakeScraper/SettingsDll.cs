namespace ImageFakeScraper;

public class SettingsDll
{

	public static long TotalPushAlamy { get; set; } = 0;
	public static long TotalPushBing { get; set; } = 0;
	public static long TotalPushGetty { get; set; } = 0;
	public static long TotalPushGoogle { get; set; } = 0;
	public static long TotalPushImmerse { get; set; } = 0;
	public static long TotalPushOpen { get; set; } = 0;
	public static long TotalPushPixel { get; set; } = 0;
	public static long TotalPushQwant { get; set; } = 0;
	public static long TotalPushUnsplash { get; set; } = 0;
	public static long TotalPushYahoo { get; set; } = 0;


	public static long downloadTotal { get; set; } = 0;

	public static long nbPushTotal { get; set; } = 0;

	public bool printLog { get; set; } = false;

	public bool printErrorLog { get; set; } = false;
}
