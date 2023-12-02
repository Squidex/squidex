// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Core.Contents;

public record Content : Entity
{
    public NamedId<DomainId> AppId { get; init; }

    public NamedId<DomainId> SchemaId { get; init; }

    public Status? NewStatus { get; init; }

    public Status Status { get; init; }

    public ContentData Data { get; set; }

    public ScheduleJob? ScheduleJob { get; init; }

    public bool IsDeleted { get; init; }

    [JsonIgnore]
    public bool IsPublished
    {
        get => Status == Status.Published;
    }
}
