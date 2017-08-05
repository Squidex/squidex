// ==========================================================================
//  DumpFormatterTests.cs
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
using Xunit;

namespace Squidex.Infrastructure.Http
{
    public class DumpFormatterTests
    {
        [Fact]
        public void Should_format_dump_without_response()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://cloud.squidex.io"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Squidex", "1.0"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("de"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("UTF-8"));

            var dump = DumpFormatter.BuildDump(request, null, null, null, TimeSpan.FromMinutes(1));

            var expected = MakeDump(
                "Request:",
                "POST: https://cloud.squidex.io/ HTTP/1.1",
                "User-Agent: Squidex/1.0",
                "Accept-Language: de; en",
                "Accept-Encoding: UTF-8",
                "",
                "",
                "Response:",
                "Timeout after 00:01:00");

            Assert.Equal(expected, dump);
        }

        [Fact]
        public void Should_format_dump_without_content()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://cloud.squidex.io"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Squidex", "1.0"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("de"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("UTF-8"));

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("UTF-8"));
            response.Headers.Trailer.Add("Expires");

            var dump = DumpFormatter.BuildDump(request, response, null, null, TimeSpan.FromMinutes(1));

            var expected = MakeDump(
                "Request:",
                "POST: https://cloud.squidex.io/ HTTP/1.1",
                "User-Agent: Squidex/1.0",
                "Accept-Language: de; en",
                "Accept-Encoding: UTF-8",
                "",
                "",
                "Response:",
                "HTTP/1.1 200 OK",
                "Transfer-Encoding: UTF-8",
                "Trailer: Expires",
                "",
                "Elapsed: 00:01:00");

            Assert.Equal(expected, dump);
        }

        [Fact]
        public void Should_format_dump_with_content()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://cloud.squidex.io"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Squidex", "1.0"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("de"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("UTF-8"));
            request.Content = new StringContent("Hello Squidex", Encoding.UTF8, "text/plain");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("UTF-8"));
            response.Headers.Trailer.Add("Expires");
            response.Content = new StringContent("Hello Back", Encoding.UTF8, "text/plain");

            var dump = DumpFormatter.BuildDump(request, response, "Hello Squidex", "Hello Back", TimeSpan.FromMinutes(1));

            var expected = MakeDump(
                "Request:",
                "POST: https://cloud.squidex.io/ HTTP/1.1",
                "User-Agent: Squidex/1.0",
                "Accept-Language: de; en",
                "Accept-Encoding: UTF-8",
                "Content-Type: text/plain; charset=utf-8",
                "",
                "Hello Squidex",
                "",
                "",
                "Response:",
                "HTTP/1.1 200 OK",
                "Transfer-Encoding: UTF-8",
                "Trailer: Expires",
                "Content-Type: text/plain; charset=utf-8",
                "",
                "Hello Back",
                "",
                "Elapsed: 00:01:00");

            Assert.Equal(expected, dump);
        }

        private static string MakeDump(params string[] input)
        {
            return string.Join(Environment.NewLine, input) + Environment.NewLine;
        }
    }
}
