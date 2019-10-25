﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure.Http;
using Squidex.Infrastructure.Json;

namespace Squidex.Extensions.Actions.Medium
{
    public sealed class MediumActionHandler : RuleActionHandler<MediumAction, MediumJob>
    {
        private const string Description = "Post to medium";

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IJsonSerializer serializer;

        private sealed class UserResponse
        {
            public UserResponseData Data { get; set; }
        }

        private sealed class UserResponseData
        {
            public string Id { get; set; }
        }

        public MediumActionHandler(RuleEventFormatter formatter, IHttpClientFactory httpClientFactory, IJsonSerializer serializer)
            : base(formatter)
        {
            this.httpClientFactory = httpClientFactory;

            this.serializer = serializer;
        }

        protected override (string Description, MediumJob Data) CreateJob(EnrichedEvent @event, MediumAction action)
        {
            var ruleJob = new MediumJob { AccessToken = action.AccessToken, PublicationId = action.PublicationId };

            var requestBody = new
            {
                title = Format(action.Title, @event),
                contentFormat = action.IsHtml ? "html" : "markdown",
                content = Format(action.Content, @event),
                canonicalUrl = Format(action.CanonicalUrl, @event),
                tags = ParseTags(@event, action)
            };

            ruleJob.RequestBody = ToJson(requestBody);

            return (Description, ruleJob);
        }

        private string[] ParseTags(EnrichedEvent @event, MediumAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Tags))
            {
                return null;
            }

            try
            {
                var jsonTags = Format(action.Tags, @event);

                return serializer.Deserialize<string[]>(jsonTags);
            }
            catch
            {
                return action.Tags.Split(',');
            }
        }

        protected override async Task<Result> ExecuteJobAsync(MediumJob job, CancellationToken ct = default)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(4);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Squidex Headless CMS");

                string path;

                if (!string.IsNullOrWhiteSpace(job.PublicationId))
                {
                    path = $"v1/publications/{job.PublicationId}/posts";
                }
                else
                {
                    HttpResponseMessage response = null;

                    var meRequest = BuildMeRequest(job);
                    try
                    {
                        response = await httpClient.SendAsync(meRequest, ct);

                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseJson = serializer.Deserialize<UserResponse>(responseString);

                        var id = responseJson.Data?.Id;

                        path = $"v1/users/{id}/posts";
                    }
                    catch (Exception ex)
                    {
                        var requestDump = DumpFormatter.BuildDump(meRequest, response, ex.ToString());

                        return Result.Failed(ex, requestDump);
                    }
                }

                return await httpClient.OneWayRequestAsync(BuildPostRequest(job, path), job.RequestBody, ct);
            }
        }

        private static HttpRequestMessage BuildPostRequest(MediumJob job, string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.medium.com/{path}")
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

        public string PublicationId { get; set; }

        public string AccessToken { get; set; }
    }
}
