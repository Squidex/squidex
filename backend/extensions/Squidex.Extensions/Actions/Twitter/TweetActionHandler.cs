﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreTweet;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions.Twitter
{
    public sealed class TweetActionHandler : RuleActionHandler<TweetAction, TweetJob>
    {
        private const string Description = "Send a tweet";

        private readonly TwitterOptions twitterOptions;

        public TweetActionHandler(RuleEventFormatter formatter, IOptions<TwitterOptions> twitterOptions)
            : base(formatter)
        {
            Guard.NotNull(twitterOptions);

            this.twitterOptions = twitterOptions.Value;
        }

        protected override (string Description, TweetJob Data) CreateJob(EnrichedEvent @event, TweetAction action)
        {
            var ruleJob = new TweetJob
            {
                Text = Format(action.Text, @event),
                AccessToken = action.AccessToken,
                AccessSecret = action.AccessSecret
            };

            return (Description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(TweetJob job, CancellationToken ct = default)
        {
            var tokens = Tokens.Create(
                twitterOptions.ClientId,
                twitterOptions.ClientSecret,
                job.AccessToken,
                job.AccessSecret);

            var request = new Dictionary<string, object>
            {
                ["status"] = job.Text
            };

            await tokens.Statuses.UpdateAsync(request, ct);

            return Result.Success($"Tweeted: {job.Text}");
        }
    }

    public sealed class TweetJob
    {
        public string AccessToken { get; set; }

        public string AccessSecret { get; set; }

        public string Text { get; set; }
    }
}
