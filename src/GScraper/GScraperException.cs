namespace GScraper;

public class GScraperException : Exception
{
    public string Engine { get; } = "Unknown";

    public GScraperException()
    {
    }

    public GScraperException(string message) : base(message)
    {
    }

    public GScraperException(string message, string engine) : this(message)
    {
        Engine = engine;
    }

    public GScraperException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public GScraperException(string message, string engine, Exception innerException) : this(message, innerException)
    {
        Engine = engine;
    }
}