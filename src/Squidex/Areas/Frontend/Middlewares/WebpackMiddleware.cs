// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Squidex.Areas.Frontend.Middlewares
{
    public sealed class WebpackMiddleware
    {
        private const string Host = "localhost";
        private const string Port = "3000";
        private static readonly string[] Scripts = { "shims", "app" };
        private static readonly string[] Styles = Array.Empty<string>();
        private readonly RequestDelegate next;

        public WebpackMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.IsHtmlPath())
            {
                var responseBuffer = new MemoryStream();
                var responseBody = context.Response.Body;

                context.Response.Body = responseBuffer;

                await next(context);

                context.Response.Body = responseBody;

                var response = Encoding.UTF8.GetString(responseBuffer.ToArray());

                if (context.IsIndex())
                {
                    response = InjectStyles(response);
                    response = InjectScripts(response);
                }

                var basePath = context.Request.PathBase;

                if (basePath.HasValue)
                {
                    response = AdjustBase(response, basePath.Value);
                }

                context.Response.ContentLength = Encoding.UTF8.GetByteCount(response);
                context.Response.Body = responseBody;

                await context.Response.WriteAsync(response);
            }
            else
            {
                await next(context);
            }
        }

        private static string InjectStyles(string response)
        {
            if (!response.Contains("</head>"))
            {
                return response;
            }

            var sb = new StringBuilder();

            foreach (var file in Styles)
            {
                sb.AppendLine($"<link href=\"http://{Host}:{Port}/{file}.css\" rel=\"stylesheet\">");
            }

            response = response.Replace("</head>", $"{sb}</head>");

            return response;
        }

        private static string InjectScripts(string response)
        {
            if (!response.Contains("</body>"))
            {
                return response;
            }

            var sb = new StringBuilder();

            foreach (var file in Scripts)
            {
                sb.AppendLine($"<script type=\"text/javascript\" src=\"http://{Host}:{Port}/{file}.js\"></script>");
            }

            response = response.Replace("</body>", $"{sb}</body>");

            return response;
        }

        private static string AdjustBase(string response, string baseUrl)
        {
            return response.Replace("<base href=\"/\">", $"<base href=\"{baseUrl}/\">");
        }
    }
}
