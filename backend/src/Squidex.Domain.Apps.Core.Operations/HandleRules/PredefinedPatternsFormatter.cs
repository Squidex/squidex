// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Shared.Identity;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class PredefinedPatternsFormatter : IRuleEventFormatter
{
    private readonly List<(string Pattern, Func<EnrichedEvent, string?> Replacer)> patterns = new List<(string Pattern, Func<EnrichedEvent, string?> Replacer)>();
    private readonly IUrlGenerator urlGenerator;

    public PredefinedPatternsFormatter(IUrlGenerator urlGenerator)
    {
        this.urlGenerator = urlGenerator;

        AddPattern("APP_ID", AppId);
        AddPattern("APP_NAME", AppName);
        AddPattern("ASSET_CONTENT_URL", AssetContentUrl);
        AddPattern("ASSET_CONTENT_APP_URL", AssetContentAppUrl);
        AddPattern("ASSET_CONTENT_SLUG_URL", AssetContentSlugUrl);
        AddPattern("CONTENT_ACTION", ContentAction);
        AddPattern("CONTENT_URL", ContentUrl);
        AddPattern("MENTIONED_ID", MentionedId);
        AddPattern("MENTIONED_NAME", MentionedName);
        AddPattern("MENTIONED_EMAIL", MentionedEmail);
        AddPattern("SCHEMA_ID", SchemaId);
        AddPattern("SCHEMA_NAME", SchemaName);
        AddPattern("TIMESTAMP_DATETIME", TimestampTime);
        AddPattern("TIMESTAMP_DATE", TimestampDate);
        AddPattern("USER_ID", UserId);
        AddPattern("USER_NAME", UserName);
        AddPattern("USER_EMAIL", UserEmail);
    }

    private void AddPattern(string placeholder, Func<EnrichedEvent, string?> generator)
    {
        patterns.Add((placeholder, generator));
    }

    public (bool Match, string?, int ReplacedLength) Format(EnrichedEvent @event, string text)
    {
        for (var j = 0; j < patterns.Count; j++)
        {
            var (pattern, replacer) = patterns[j];

            if (text.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            {
                var result = replacer(@event);

                return (true, result, pattern.Length);
            }
        }

        return default;
    }

    private static string TimestampDate(EnrichedEvent @event)
    {
        return @event.Timestamp.ToDateTimeUtc().ToString("yyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string TimestampTime(EnrichedEvent @event)
    {
        return @event.Timestamp.ToDateTimeUtc().ToString("yyy-MM-dd-hh-mm-ss", CultureInfo.InvariantCulture);
    }

    private static string AppId(EnrichedEvent @event)
    {
        return @event.AppId.Id.ToString();
    }

    private static string AppName(EnrichedEvent @event)
    {
        return @event.AppId.Name;
    }

    private static string? SchemaId(EnrichedEvent @event)
    {
        if (@event is EnrichedSchemaEventBase schemaEvent)
        {
            return schemaEvent.SchemaId.Id.ToString();
        }

        return null;
    }

    private static string? SchemaName(EnrichedEvent @event)
    {
        if (@event is EnrichedSchemaEventBase schemaEvent)
        {
            return schemaEvent.SchemaId.Name;
        }

        return null;
    }

    private static string? ContentAction(EnrichedEvent @event)
    {
        if (@event is EnrichedContentEvent contentEvent)
        {
            return contentEvent.Type.ToString();
        }

        return null;
    }

    private string? AssetContentUrl(EnrichedEvent @event)
    {
        if (@event is EnrichedAssetEvent assetEvent)
        {
            return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString());
        }

        return null;
    }

    private string? AssetContentAppUrl(EnrichedEvent @event)
    {
        if (@event is EnrichedAssetEvent assetEvent)
        {
            return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.Id.ToString());
        }

        return null;
    }

    private string? AssetContentSlugUrl(EnrichedEvent @event)
    {
        if (@event is EnrichedAssetEvent assetEvent)
        {
            return urlGenerator.AssetContent(assetEvent.AppId, assetEvent.FileName.Slugify());
        }

        return null;
    }

    private string? ContentUrl(EnrichedEvent @event)
    {
        if (@event is EnrichedContentEvent contentEvent)
        {
            return urlGenerator.ContentUI(contentEvent.AppId, contentEvent.SchemaId, contentEvent.Id);
        }

        return null;
    }

    private static string? UserName(EnrichedEvent @event)
    {
        if (@event is EnrichedUserEventBase userEvent)
        {
            return userEvent.User?.Claims.DisplayName();
        }

        return null;
    }

    private static string? UserId(EnrichedEvent @event)
    {
        if (@event is EnrichedUserEventBase userEvent)
        {
            return userEvent.User?.Id;
        }

        return null;
    }

    private static string? UserEmail(EnrichedEvent @event)
    {
        if (@event is EnrichedUserEventBase userEvent)
        {
            return userEvent.User?.Email;
        }

        return null;
    }

    private static string? MentionedName(EnrichedEvent @event)
    {
        if (@event is EnrichedCommentEvent commentEvent)
        {
            return commentEvent.MentionedUser.Claims.DisplayName();
        }

        return null;
    }

    private static string? MentionedId(EnrichedEvent @event)
    {
        if (@event is EnrichedCommentEvent commentEvent)
        {
            return commentEvent.MentionedUser.Id;
        }

        return null;
    }

    private static string? MentionedEmail(EnrichedEvent @event)
    {
        if (@event is EnrichedCommentEvent commentEvent)
        {
            return commentEvent.MentionedUser.Email;
        }

        return null;
    }
}
