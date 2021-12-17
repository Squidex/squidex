// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Images.Models;

namespace Squidex.Areas.Api.Controllers.Images.Service
{
    public sealed class RemoteImageResizer : IImageResizer
    {
        private readonly IHttpClientFactory httpClientFactory;

        public RemoteImageResizer(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> ResizeAsync(ResizeRequest request,
            CancellationToken ct = default)
        {
            using (var httpClient = httpClientFactory.CreateClient("ImageResizer"))
            {
                var response = await httpClient.PostAsJsonAsync("/images/resize", request, ct);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<ResizeResponse>(cancellationToken: ct);

                return json!.ResultPath;
            }
        }
    }
}
