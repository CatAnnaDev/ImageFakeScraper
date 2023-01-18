namespace ImageFakeScraper;

public class SettingsDll
{
	public static long downloadTotal { get; set; } = 0;

	public static long nbPushTotal { get; set; } = 0;

	public bool printLog { get; set; } = false;

	public bool printErrorLog { get; set; } = false;
}
