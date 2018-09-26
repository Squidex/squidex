// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Discourse
{
    public sealed class DiscourseActionHandler : RuleActionHandler<DiscourseAction, DiscourseJob>
    {
        private const string DescriptionCreatePost = "Create discourse Post";
        private const string DescriptionCreateTopic = "Create discourse Topic";

        private readonly IHttpClientFactory httpClientFactory;

        public DiscourseActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected override (string Description, DiscourseJob Data) CreateJob(EnrichedEvent @event, DiscourseAction action)
        {
            var url = $"{action.Url.ToString().TrimEnd('/')}/posts.json?api_key={action.ApiKey}&api_username={action.ApiUsername}";

            var json =
                new JObject(
                    new JProperty("raw", Format(action.Text, @event)),
                    new JProperty("title", Format(action.Title, @event)));

            if (action.Topic.HasValue)
            {
                json.Add(new JProperty("topic_id", action.Topic.Value));
            }

            if (action.Category.HasValue)
            {
                json.Add(new JProperty("category", action.Category.Value));
            }

            var ruleJob = new DiscourseJob
            {
                RequestUrl = url,
                RequestBody = json.ToString()
            };

            var description =
                action.Topic.HasValue ?
                DescriptionCreateTopic :
                DescriptionCreatePost;

            return (description, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(DiscourseJob job)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, job.RequestUrl)
                {
                    Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
                };

                return await httpClient.OneWayRequestAsync(request, job.RequestBody);
            }
        }
    }

    public sealed class DiscourseJob
    {
        public string RequestUrl { get; set; }

        public string RequestBody { get; set; }
    }
}
