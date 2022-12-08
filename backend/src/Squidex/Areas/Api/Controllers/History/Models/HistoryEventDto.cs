// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.History.Models;

public sealed class HistoryEventDto
{
    /// <summary>
    /// The message for the event.
    /// </summary>
    [LocalizedRequired]
    public string Message { get; set; }

    /// <summary>
    /// The type of the original event.
    /// </summary>
    [LocalizedRequired]
    public string EventType { get; set; }

    /// <summary>
    /// The user who called the action.
    /// </summary>
    [LocalizedRequired]
    public string Actor { get; set; }

    /// <summary>
    /// Gets a unique id for the event.
    /// </summary>
    public DomainId EventId { get; set; }

    /// <summary>
    /// The time when the event happened.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The version identifier.
    /// </summary>
    public long Version { get; set; }

    public static HistoryEventDto FromDomain(ParsedHistoryEvent historyEvent)
    {
        var result = SimpleMapper.Map(historyEvent, new HistoryEventDto { EventId = historyEvent.Id });

        return result;
    }
}
