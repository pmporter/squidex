﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using System.Web;
using Squidex.Infrastructure;

namespace Squidex.Pipeline.Squid;

#pragma warning disable CS9113 // Parameter is unread.
public sealed class SquidMiddleware(RequestDelegate next)
#pragma warning restore CS9113 // Parameter is unread.
{
    private readonly string squidHappyLG = LoadSvg("happy");
    private readonly string squidHappySM = LoadSvg("happy-sm");
    private readonly string squidSadLG = LoadSvg("sad");
    private readonly string squidSadSM = LoadSvg("sad-sm");

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        string? requestedFace = null;
        string? requestedTitle = null;
        string? requestedText = null;
        string? requestedBackground = null;

        if (request.Query.TryGetValue("face", out var faceValue))
        {
            requestedFace = faceValue;
        }

        if (request.Query.TryGetValue("title", out var titleValue))
        {
            requestedTitle = titleValue!;
        }

        if (request.Query.TryGetValue("text", out var textValue))
        {
            requestedText = textValue!;
        }

        if (request.Query.TryGetValue("background", out var backgroundValue))
        {
            requestedBackground = backgroundValue;
        }

        if (string.IsNullOrWhiteSpace(requestedFace) || requestedFace is not "sad" and not "happy")
        {
            requestedFace = "sad";
        }

        var isSad = string.Equals(requestedFace, "sad", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(requestedTitle))
        {
            requestedTitle = isSad ? "OH DAMN!" : "OH YEAH!";
        }

        if (string.IsNullOrWhiteSpace(requestedText))
        {
            requestedText = "text";
        }

        if (string.IsNullOrWhiteSpace(requestedBackground))
        {
            requestedBackground = isSad ? "#F5F5F9" : "#4CC159";
        }

        var isSmall = request.Query.TryGetValue("small", out _);

        string svg;

        if (isSmall)
        {
            svg = isSad ? squidSadSM : squidHappySM;
        }
        else
        {
            svg = isSad ? squidSadLG : squidHappyLG;
        }

        var (line1, line2, line3) = SplitText(requestedText);

        void Replace(string source, string value)
        {
            svg = svg.Replace(source, HttpUtility.HtmlEncode(value), StringComparison.Ordinal);
        }

        Replace("{{TITLE}}", requestedTitle.ToUpperInvariant());
        Replace("{{TEXT1}}", line1);
        Replace("{{TEXT2}}", line2);
        Replace("{{TEXT3}}", line3);
        Replace("[COLOR]", requestedBackground);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "image/svg+xml";
        context.Response.Headers.CacheControl = "public, max-age=604800";

        await context.Response.WriteAsync(svg, context.RequestAborted);
    }

    private static (string, string, string) SplitText(string text)
    {
        var result = new List<string>();

        var line = new StringBuilder();

        foreach (var word in text.Split(' '))
        {
            if (line.Length + word.Length > 16 && line.Length > 0)
            {
                result.Add(line.ToString());

                line.Clear();
            }

            line.AppendIfNotEmpty(' ');
            line.Append(word);
        }

        result.Add(line.ToString());

        while (result.Count < 3)
        {
            result.Add(string.Empty);
        }

        return (result[0], result[1], result[2]);
    }

    private static string LoadSvg(string name)
    {
        var assembly = typeof(SquidMiddleware).Assembly;

        using (var resourceStream = assembly.GetManifestResourceStream($"Squidex.Pipeline.Squid.icon-{name}.svg"))
        {
            using (var streamReader = new StreamReader(resourceStream!))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
