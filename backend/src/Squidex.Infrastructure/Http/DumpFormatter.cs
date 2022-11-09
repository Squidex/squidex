// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Squidex.Infrastructure.Http;

public static class DumpFormatter
{
    public static string BuildDump(HttpRequestMessage request, HttpResponseMessage? response, string? responseBody)
    {
        return BuildDump(request, response, null, responseBody, TimeSpan.Zero);
    }

    public static string BuildDump(HttpRequestMessage request, HttpResponseMessage? response, string? requestBody, string? responseBody)
    {
        return BuildDump(request, response, requestBody, responseBody, TimeSpan.Zero);
    }

    public static string BuildDump(HttpRequestMessage request, HttpResponseMessage? response, string? requestBody, string? responseBody, TimeSpan elapsed, bool isTimeout = false)
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

    private static void AppendRequest(this StringBuilder writer, HttpRequestMessage request, string? requestBody)
    {
        var method = request.Method.ToString().ToUpperInvariant();

        writer.AppendLine(CultureInfo.InvariantCulture, $"{method}: {request.RequestUri} HTTP/{request.Version}");

        writer.AppendHeaders(request.Headers);
        writer.AppendHeaders(request.Content?.Headers);

        if (!string.IsNullOrWhiteSpace(requestBody))
        {
            writer.AppendLine();
            writer.AppendLine(requestBody);
        }
    }

    private static void AppendResponse(this StringBuilder writer, HttpResponseMessage? response, string? responseBody, TimeSpan elapsed, bool isTimeout)
    {
        if (response != null)
        {
            var responseCode = (int)response.StatusCode;
            var responseText = Enum.GetName(typeof(HttpStatusCode), response.StatusCode);

            writer.AppendLine(CultureInfo.InvariantCulture, $"HTTP/{response.Version} {responseCode} {responseText}");

            writer.AppendHeaders(response.Headers);
            writer.AppendHeaders(response.Content?.Headers);
        }

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            writer.AppendLine();
            writer.AppendLine(responseBody);
        }

        if (response != null && elapsed != TimeSpan.Zero)
        {
            writer.AppendLine();
            writer.AppendLine(CultureInfo.InvariantCulture, $"Elapsed: {elapsed}");
        }

        if (isTimeout)
        {
            writer.AppendLine(CultureInfo.InvariantCulture, $"Timeout after {elapsed}");
        }
    }

    private static void AppendHeaders(this StringBuilder writer, HttpHeaders? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var (key, value) in headers)
        {
            writer.Append(key);
            writer.Append(": ");
            writer.Append(string.Join("; ", value));
            writer.AppendLine();
        }
    }
}
