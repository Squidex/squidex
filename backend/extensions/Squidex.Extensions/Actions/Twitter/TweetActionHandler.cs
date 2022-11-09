// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using CoreTweet;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Twitter;

public sealed class TweetActionHandler : RuleActionHandler<TweetAction, TweetJob>
{
    private const string Description = "Send a tweet";

    private readonly TwitterOptions twitterOptions;

    public TweetActionHandler(RuleEventFormatter formatter, IOptions<TwitterOptions> twitterOptions)
        : base(formatter)
    {
        this.twitterOptions = twitterOptions.Value;
    }

    protected override async Task<(string Description, TweetJob Data)> CreateJobAsync(EnrichedEvent @event, TweetAction action)
    {
        var ruleJob = new TweetJob
        {
            Text = await FormatAsync(action.Text, @event),
            AccessToken = action.AccessToken,
            AccessSecret = action.AccessSecret
        };

        return (Description, ruleJob);
    }

    protected override async Task<Result> ExecuteJobAsync(TweetJob job,
        CancellationToken ct = default)
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
