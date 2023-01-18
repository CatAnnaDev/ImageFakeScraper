namespace ImageFakeScraper;
#pragma warning disable CS8602, CS8604, CS8618, CS1634
public class ImageFakeScraperException : Exception
{
    public string Engine { get; } = "Unknown";

    public ImageFakeScraperException()
    {
    }

    public ImageFakeScraperException(string message) : base(message)
    {
    }

    public ImageFakeScraperException(string message, string engine) : this(message)
    {
        Engine = engine;
    }

    public ImageFakeScraperException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ImageFakeScraperException(string message, string engine, Exception innerException) : this(message, innerException)
    {
        Engine = engine;
    }
}