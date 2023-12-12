// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents;

public record Content : AppEntity
{
    public NamedId<DomainId> SchemaId { get; init; }

    public Status? NewStatus { get; init; }

    public Status Status { get; init; }

    public ContentData Data { get; set; }

    public ScheduleJob? ScheduleJob { get; init; }

    public bool IsPublished
    {
        get => Status == Status.Published;
    }
}
