// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities;

public static class EntityExtensions
{
    public static T Apply<T>(this T source, Envelope<SquidexEvent> @event) where T : Entity
    {
        var headers = @event.Headers;

        var timestamp = headers.Timestamp();
        var created = source.Created;
        var createdBy = source.CreatedBy;

        if (created == default)
        {
            created = timestamp;
        }

        if (createdBy == null)
        {
            createdBy = @event.Payload.Actor;
        }

        return source with
        {
            Created = created,
            CreatedBy = createdBy,
            LastModified = timestamp,
            LastModifiedBy = @event.Payload.Actor
        };
    }
}
