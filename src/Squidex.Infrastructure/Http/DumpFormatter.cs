// ==========================================================================
//  DumpFormatter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.Http
{
    public static class DumpFormatter
    {
        public static string BuildDump(HttpRequestMessage request, HttpResponseMessage response, string requestBody, string responseBody, TimeSpan elapsed, bool isTimeout)
        {
            var writer = new StringBuilder();

            writer.AppendLine("Request:");
            writer.AppendRequest(request, requestBody);

            writer.AppendLine();
            writer.AppendLine();

            writer.AppendLine("Response:");
            writer.AppendResponse(response, responseBody, elapsed, isTimeout);

            return writer.ToString();
        }

        private static void AppendRequest(this StringBuilder writer, HttpRequestMessage request, string requestBody)
        {
            var method = request.Method.ToString().ToUpperInvariant();

            writer.AppendLine($"{method}: {request.RequestUri} HTTP/{request.Version}");

            writer.AppendHeaders(request.Headers);
            writer.AppendHeaders(request.Content?.Headers);

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                writer.AppendLine();
                writer.AppendLine(requestBody);
            }
        }

        private static void AppendResponse(this StringBuilder writer, HttpResponseMessage response, string responseBody, TimeSpan elapsed, bool isTimeout)
        {
            if (response != null)
            {
                var responseCode = (int)response.StatusCode;
                var responseText = Enum.GetName(typeof(HttpStatusCode), response.StatusCode);

                writer.AppendLine($"HTTP/{response.Version} {responseCode} {responseText}");

                writer.AppendHeaders(response.Headers);
                writer.AppendHeaders(response.Content?.Headers);
            }

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                writer.AppendLine();
                writer.AppendLine(responseBody);
            }

            if (response != null)
            {
                writer.AppendLine();
                writer.AppendLine($"Elapsed: {elapsed}");
            }

            if (isTimeout)
            {
                writer.AppendLine($"Timeout after {elapsed}");
            }
        }

        private static void AppendHeaders(this StringBuilder writer, HttpHeaders headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                writer.Append(header.Key);
                writer.Append(": ");
                writer.Append(string.Join("; ", header.Value));
                writer.AppendLine();
            }
        }
    }
}
