// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Scripting
{
    internal sealed class MockupHttpHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage response;
        private HttpRequestMessage madeRequest;

        public void ShouldBeMethod(HttpMethod method)
        {
            Assert.Equal(method, madeRequest.Method);
        }

        public void ShouldBeUrl(string url)
        {
            Assert.Equal(url, madeRequest.RequestUri?.ToString());
        }

        public void ShouldBeHeader(string key, string value)
        {
            Assert.Equal(value, madeRequest.Headers.GetValues(key).FirstOrDefault());
        }

        public MockupHttpHandler(HttpResponseMessage response)
        {
            this.response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);

            madeRequest = request;

            return response;
        }
    }
}
