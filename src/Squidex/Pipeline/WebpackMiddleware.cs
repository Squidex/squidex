// ==========================================================================
//  WebpackMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// ReSharper disable LoopCanBeConvertedToQuery

namespace Squidex.Pipeline
{
    public sealed class WebpackMiddleware
    {
        private const string Host = "localhost";
        private const string Port = "3000";
        private static readonly string[] Scripts = { "polyfills.js", "vendor.js", "app.js" };
        private static readonly string[] Styles = { "vendor.css" };
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public WebpackMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<WebpackMiddleware>();
            
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var buffer = new MemoryStream();
            var body = context.Response.Body;

            context.Response.Body = buffer;

            await next(context);

            buffer.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode == 200 && IsIndex(context) && IsHtml(context))
            {
                using (var reader = new StreamReader(buffer))
                {
                    var response = await reader.ReadToEndAsync();

                    response = InjectStyles(response);
                    response = InjectScripts(response);

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(memoryStream))
                        {
                            writer.Write(response);
                            writer.Flush();

                            memoryStream.Seek(0, SeekOrigin.Begin);

                            context.Response.Headers["Content-Length"] = memoryStream.Length.ToString();

                            await memoryStream.CopyToAsync(body);
                        }
                    }
                }
            }
            else if (context.Response.StatusCode != 304)
            {
                await buffer.CopyToAsync(body);
            }

            context.Response.Body = body;
        }

        private string InjectStyles(string response)
        {
            if (!response.Contains("</head>"))
            {
                return response;
            }

            logger.LogInformation("A full html page is returned so the necessary styles for webpack will be injected");

            var stylesTag = string.Empty;

            foreach (var file in Styles)
            {
                stylesTag += $"<link href=\"http://{Host}:{Port}/{file}\" rel=\"stylesheet\">";
            }

            response = response.Replace("</head>", $"{stylesTag}</head>");

            logger.LogInformation($"Inject style {stylesTag} as a last element in the head ");

            return response;
        }

        private string InjectScripts(string response)
        {
            if (!response.Contains("</body>"))
            {
                return response;
            }

            logger.LogInformation("A full html page is returned so the necessary script for webpack will be injected");

            var scriptsTag = string.Empty;

            foreach (var file in Scripts)
            {
                scriptsTag += $"<script type=\"text/javascript\" src=\"http://{Host}:{Port}/{file}\"></script>";
            }

            response = response.Replace("</body>", $"{scriptsTag}</body>");

            logger.LogInformation($"Inject script {scriptsTag} as a last element in the body ");

            return response;
        }

        private static bool IsIndex(HttpContext context)
        {
            return context.Request.Path.Value.Equals("/index.html", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHtml(HttpContext context)
        {
            return context.Response.ContentType?.ToLower().Contains("text/html") == true;
        }
    }
}
