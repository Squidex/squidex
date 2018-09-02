// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Discourse
{
    public sealed class DiscourseActionHandler : RuleActionHandler<DiscourseAction, DiscourseJob>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DiscourseActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected override Task<(string Description, DiscourseJob Data)> CreateJobAsync(EnrichedEvent @event, DiscourseAction action)
        {
            return base.CreateJobAsync(@event, action);
        }

        protected override Task<(string Dump, Exception Exception)> ExecuteJobAsync(DiscourseJob job)
        {
            using (var client = httpClientFactory.CreateClient())
            {
                // Foo
            }

            return Task.FromResult<(string Dump, Exception Exception)>((string.Empty, null));
        }
    }

    public sealed class DiscourseJob
    {
        public string RequestUrl { get; set; }

        public string RequestBody { get; set; }
    }
}
