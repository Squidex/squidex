// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using CoreTweet;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure;

#pragma warning disable SA1649 // File name must match first type name

namespace Squidex.Domain.Apps.Core.HandleRules.Actions
{
    public sealed class TweetJob
    {
        public string PinCode { get; set; }

        public string Text { get; set; }
    }

    public sealed class TweetActionHandler : RuleActionHandler<TweetAction, TweetJob>
    {
        private const string Description = "Send a tweet";

        private readonly RuleEventFormatter formatter;
        private readonly ClientPool<string, Tokens> tokenPool;

        public TweetActionHandler(RuleEventFormatter formatter, IOptions<TwitterOptions> twitterOptions)
        {
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(twitterOptions, nameof(twitterOptions));

            this.formatter = formatter;

            tokenPool = new ClientPool<string, Tokens>(async key =>
            {
                var session = await OAuth.AuthorizeAsync(twitterOptions.Value.ClientId, twitterOptions.Value.ClientSecret);

                return await session.GetTokensAsync(key);
            });
        }

        protected override (string Description, TweetJob Data) CreateJob(EnrichedEvent @event, TweetAction action)
        {
            var text = formatter.Format(action.Text, @event);

            var ruleJob = new TweetJob { Text = text, PinCode = action.PinCode };

            return (Description, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(TweetJob job)
        {
            try
            {
                var tokens = await tokenPool.GetClientAsync(job.PinCode);

                var response = await tokens.Statuses.UpdateAsync(x => job.Text);

                return ($"Tweeted: {job.Text}", null);
            }
            catch (Exception ex)
            {
                return (ex.Message, ex);
            }
        }
    }
}
