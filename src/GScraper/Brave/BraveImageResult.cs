using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper.Brave;

/// <summary>
/// Represents an image result from Brave.
/// </summary>
[DebuggerDisplay("Title: {Title}, Url: {Url}")]
public class BraveImageResult : IImageResult
{
    internal BraveImageResult(BraveImageProperties properties)
    {
        Url = properties.Url;
    }

    public string Url { get; }

    public override string ToString() => Url;
}