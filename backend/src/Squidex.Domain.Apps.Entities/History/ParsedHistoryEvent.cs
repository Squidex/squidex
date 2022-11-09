// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.History;

public sealed class ParsedHistoryEvent
{
    private readonly HistoryEvent item;
    private readonly Lazy<string?> message;

    public DomainId Id
    {
        get => item.Id;
    }

    public Instant Created
    {
        get => item.Created;
    }

    public RefToken Actor
    {
        get => item.Actor;
    }

    public long Version
    {
        get => item.Version;
    }

    public string Channel
    {
        get => item.Channel;
    }

    public string EventType
    {
        get => item.EventType;
    }

    public string? Message
    {
        get => message.Value;
    }

    public ParsedHistoryEvent(HistoryEvent item, IReadOnlyDictionary<string, string> texts)
    {
        this.item = item;

        message = new Lazy<string?>(() =>
        {
            if (texts.TryGetValue(item.EventType, out var translationKey))
            {
                var result = T.Get(translationKey);

                foreach (var (key, value) in item.Parameters)
                {
                    result = result.Replace("[" + key + "]", value, StringComparison.Ordinal);
                }

                return result;
            }

            return null;
        });
    }
}
