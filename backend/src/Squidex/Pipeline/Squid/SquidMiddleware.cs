// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Squidex.Pipeline.Squid
{
    public sealed class SquidMiddleware
    {
        private readonly string squidHappyLG = LoadSvg("happy");
        private readonly string squidHappySM = LoadSvg("happy-sm");
        private readonly string squidSadLG = LoadSvg("sad");
        private readonly string squidSadSM = LoadSvg("sad-sm");

        public SquidMiddleware(RequestDelegate next)
        {
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            var face = "sad";

            if (request.Query.TryGetValue("face", out var faceValue) && (faceValue == "sad" || faceValue == "happy"))
            {
                face = faceValue;
            }

            var isSad = face == "sad";

            var title = isSad ? "OH DAMN!" : "OH YEAH!";

            if (request.Query.TryGetValue("title", out var titleValue) && !string.IsNullOrWhiteSpace(titleValue))
            {
                title = titleValue;
            }

            var text = "text";

            if (request.Query.TryGetValue("text", out var textValue) && !string.IsNullOrWhiteSpace(textValue))
            {
                text = textValue;
            }

            var background = isSad ? "#F5F5F9" : "#4CC159";

            if (request.Query.TryGetValue("background", out var backgroundValue) && !string.IsNullOrWhiteSpace(backgroundValue))
            {
                background = backgroundValue;
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

            var (l1, l2, l3) = SplitText(text);

            svg = svg.Replace("{{TITLE}}", title.ToUpperInvariant(), StringComparison.Ordinal);
            svg = svg.Replace("{{TEXT1}}", l1, StringComparison.Ordinal);
            svg = svg.Replace("{{TEXT2}}", l2, StringComparison.Ordinal);
            svg = svg.Replace("{{TEXT3}}", l3, StringComparison.Ordinal);
            svg = svg.Replace("[COLOR]", background, StringComparison.Ordinal);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "image/svg+xml";
            context.Response.Headers["Cache-Control"] = "public, max-age=604800";

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

                if (line.Length > 0)
                {
                    line.Append(' ');
                }

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
}
