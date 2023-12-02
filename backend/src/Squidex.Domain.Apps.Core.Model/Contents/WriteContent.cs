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

public record WriteContent : Entity
{
    public NamedId<DomainId> AppId { get; init; }

    public NamedId<DomainId> SchemaId { get; init; }

    public ContentVersion? NewVersion { get; init; }

    public ContentVersion CurrentVersion { get; init; }

    public ScheduleJob? ScheduleJob { get; init; }

    public bool IsDeleted { get; init; }

    [JsonIgnore]
    public ContentData EditingData
    {
        get => NewVersion?.Data ?? CurrentVersion.Data;
    }

    [JsonIgnore]
    public Status EditingStatus
    {
        get => NewVersion?.Status ?? CurrentVersion.Status;
    }

    [JsonIgnore]
    public bool IsPublished
    {
        get => (NewVersion?.Status ?? CurrentVersion?.Status ?? default) == Status.Published;
    }

    public Content ToContent()
    {
        return new Content
        {
            Id = Id,
            AppId = AppId,
            Created = Created,
            CreatedBy = CreatedBy,
            Data = NewVersion?.Data ?? CurrentVersion.Data,
            IsDeleted = IsDeleted,
            LastModified = LastModified,
            LastModifiedBy = LastModifiedBy,
            NewStatus = NewVersion?.Status,
            ScheduleJob = ScheduleJob,
            SchemaId = SchemaId,
            Status = CurrentVersion.Status,
            UniqueId = UniqueId,
            Version = Version
        };
    }
}
