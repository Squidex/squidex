// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1649 // File name must match first type name

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Http;

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class MediumJob
    {
        public string RequestBody { get; set; }

        public string AccessToken { get; set; }
    }

    public sealed class MediumActionHandler : RuleActionHandler<MediumAction, MediumJob>
    {
        private const string Description = "Post to medium";

        private readonly RuleEventFormatter formatter;

        public MediumActionHandler(RuleEventFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            this.formatter = formatter;
        }

        protected override (string Description, MediumJob Data) CreateJob(EnrichedEvent @event, MediumAction action)
        {
            var requestBody =
                new JObject(
                    new JProperty("title", formatter.Format(action.Title, @event)),
                    new JProperty("contentFormat", action.IsHtml ? "html" : "markdown"),
                    new JProperty("content", formatter.Format(action.Content, @event)),
                    new JProperty("canonicalUrl", formatter.Format(action.CanonicalUrl, @event)),
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
                var jsonTags = formatter.Format(action.Tags, @event);

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
            string id;

            HttpResponseMessage response = null;

            var meRequest = BuildMeRequest(job);
            try
            {
                response = await HttpClientPool.GetHttpClient().SendAsync(meRequest);

                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JToken.Parse(responseString);

                id = responseJson["data"]["id"].ToString();
            }
            catch (Exception ex)
            {
                var requestDump = DumpFormatter.BuildDump(meRequest, response, ex.ToString());

                return (requestDump, ex);
            }

            var postRequestBody = job.RequestBody;
            var postRequest = BuildPostRequest(job, postRequestBody, id);

            try
            {
                response = await HttpClientPool.GetHttpClient().SendAsync(postRequest);

                var responseString = await response.Content.ReadAsStringAsync();
                var requestDump = DumpFormatter.BuildDump(postRequest, response, postRequestBody, responseString);

                Exception ex = null;

                if (!response.IsSuccessStatusCode)
                {
                    ex = new HttpRequestException($"Response code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                }

                return (requestDump, ex);
            }
            catch (Exception ex)
            {
                var requestDump = DumpFormatter.BuildDump(postRequest, response, postRequestBody, ex.ToString());

                return (requestDump, ex);
            }
        }

        private static HttpRequestMessage BuildPostRequest(MediumJob job, string requestBody, string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.medium.com/v1/users/{id}/posts")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Authorization", $"Bearer {job.AccessToken}");

            return request;
        }

        private static HttpRequestMessage BuildMeRequest(MediumJob job)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.medium.com/v1/me");

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Authorization", $"Bearer {job.AccessToken}");

            return request;
        }
    }
}
