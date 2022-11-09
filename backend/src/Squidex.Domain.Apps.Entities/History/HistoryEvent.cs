// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History;

public sealed class HistoryEvent
{
    public DomainId Id { get; set; } = DomainId.NewGuid();

    public DomainId OwnerId { get; set; }

    public Instant Created { get; set; }

    public RefToken Actor { get; set; }

    public long Version { get; set; }

    public string Channel { get; set; }

    public string EventType { get; set; }

    public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

    public HistoryEvent()
    {
    }

    public HistoryEvent(string channel, string eventType)
    {
        Guard.NotNullOrEmpty(channel);
        Guard.NotNullOrEmpty(eventType);

        Channel = channel;

        EventType = eventType;
    }

    public HistoryEvent Param(string key, object? value)
    {
        var formatted = value?.ToString();

        if (formatted != null)
        {
            Parameters[key] = formatted;
        }

        return this;
    }
}
