// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Cloud
{
    public sealed class FastlyInvalidator : IEventConsumer
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;
        private readonly string fastlyApiKey;
        private readonly string fastlyServiceId;

        public string Name
        {
            get { return "Fastly"; }
        }

        public string EventsFilter
        {
            get { return ".*"; }
        }

        public FastlyInvalidator(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;

            this.fastlyApiKey = config.GetValue<string>("fastly:apiKey");
            this.fastlyServiceId = config.GetValue<string>("fastly:serviceId");
        }

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case AssetEvent c:
                    await InvalidateAsync(c.AssetId);
                    break;
                case ContentEvent c:
                    await InvalidateAsync(c.ContentId);
                    break;
                case AppLanguageAdded a:
                    await InvalidateAsync(a.AppId.Id);
                    break;
                case AppLanguageUpdated a:
                    await InvalidateAsync(a.AppId.Id);
                    break;
                case AppLanguageRemoved a:
                    await InvalidateAsync(a.AppId.Id);
                    break;
            }
        }

        private async Task InvalidateAsync(DomainId key)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(2);

                var requestUrl = $"https://api.fastly.com/service/{fastlyServiceId}/purge/{key}";
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                request.Headers.Add("Fastly-Key", fastlyApiKey);

                using (var response = await httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
