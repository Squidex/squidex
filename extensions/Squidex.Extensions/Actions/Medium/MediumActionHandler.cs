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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure.Http;

namespace Squidex.Extensions.Actions.Medium
{
    public sealed class MediumActionHandler : RuleActionHandler<MediumAction, MediumJob>
    {
        private const string Description = "Post to medium";

        private readonly IHttpClientFactory httpClientFactory;

        public MediumActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory)
            : base(formatter)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected override (string Description, MediumJob Data) CreateJob(EnrichedEvent @event, MediumAction action)
        {
            var requestBody =
                new JObject(
                    new JProperty("title", Format(action.Title, @event)),
                    new JProperty("contentFormat", action.IsHtml ? "html" : "markdown"),
                    new JProperty("content", Format(action.Content, @event)),
                    new JProperty("canonicalUrl", Format(action.CanonicalUrl, @event)),
                    new JProperty("tags", ParseTags(@event, action)));

            var ruleJob = new MediumJob { AccessToken = action.AccessToken, RequestBody = requestBody.ToString(Formatting.Indented) };

            return (Description, ruleJob);
        }

        private JArray ParseTags(EnrichedEvent @event, MediumAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Tags))
            {
                return null;
            }

            string[] tags;
            try
            {
                var jsonTags = Format(action.Tags, @event);

                tags = JsonConvert.DeserializeObject<string[]>(jsonTags);
            }
            catch
            {
                tags = action.Tags.Split(',');
            }

            return new JArray(tags);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(MediumJob job)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(4);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Squidex Headless CMS");

                string id;

                HttpResponseMessage response = null;

                var meRequest = BuildMeRequest(job);
                try
                {
                    response = await httpClient.SendAsync(meRequest);

                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseJson = JToken.Parse(responseString);

                    id = responseJson["data"]["id"].ToString();
                }
                catch (Exception ex)
                {
                    var requestDump = DumpFormatter.BuildDump(meRequest, response, ex.ToString());

                    return (requestDump, ex);
                }

                return await httpClient.OneWayRequestAsync(BuildPostRequest(job, id), job.RequestBody);
            }
        }

        private static HttpRequestMessage BuildPostRequest(MediumJob job, string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.medium.com/v1/users/{id}/posts")
            {
                Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {job.AccessToken}");

            return request;
        }

        private static HttpRequestMessage BuildMeRequest(MediumJob job)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.medium.com/v1/me");

            request.Headers.Add("Authorization", $"Bearer {job.AccessToken}");

            return request;
        }
    }

    public sealed class MediumJob
    {
        public string RequestBody { get; set; }

        public string AccessToken { get; set; }
    }
}
