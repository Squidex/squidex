// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents;

public record EFContentCompleteEntity : EFContentEntity
{
    public static async Task<(EFContentCompleteEntity, EFReferenceCompleteEntity[])> CreateAsync(
        SnapshotWriteJob<WriteContent> job,
        IAppProvider appProvider,
        CancellationToken ct)
    {
        var source = job.Value;

        var appId = source.AppId.Id;

        var (referencedIds, translationStatus) = await CreateExtendedValuesAsync(source, source.CurrentVersion.Data, appProvider, ct);
        var references =
            referencedIds
                .Select(x => new EFReferenceCompleteEntity
                {
                    AppId = appId,
                    FromKey = job.Key,
                    FromSchema = job.Value.SchemaId.Id,
                    ToId = x,
                })
                .ToArray();

        var entity = new EFContentCompleteEntity
        {
            Id = source.Id,
            AppId = source.AppId,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Data = source.EditingData,
            DocumentId = job.Key,
            IndexedAppId = source.AppId.Id,
            IndexedSchemaId = source.SchemaId.Id,
            IsDeleted = source.IsDeleted,
            LastModified = source.LastModified,
            LastModifiedBy = source.LastModifiedBy,
            NewData = source.NewVersion != null ? source.CurrentVersion.Data : null,
            NewStatus = source.NewVersion?.Status,
            ScheduledAt = source.ScheduleJob?.DueTime,
            ScheduleJob = source.ScheduleJob,
            SchemaId = source.SchemaId,
            Status = source.CurrentVersion.Status,
            TranslationStatus = translationStatus,
            Version = source.Version,
        };

        return (entity, references);
    }
}

public record EFContentPublishedEntity : EFContentEntity
{
    public static async Task<(EFContentPublishedEntity, EFReferencePublishedEntity[])> CreateAsync(
        SnapshotWriteJob<WriteContent> job,
        IAppProvider appProvider,
        CancellationToken ct)
    {
        var source = job.Value;

        var appId = source.AppId.Id;

        var (referencedIds, translationStatus) = await CreateExtendedValuesAsync(source, source.CurrentVersion.Data, appProvider, ct);
        var references =
            referencedIds
                .Select(x => new EFReferencePublishedEntity
                {
                    AppId = appId,
                    FromKey = job.Key,
                    FromSchema = job.Value.SchemaId.Id,
                    ToId = x,
                })
                .ToArray();

        var entity = new EFContentPublishedEntity
        {
            Id = source.Id,
            AppId = source.AppId,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Data = source.CurrentVersion.Data,
            DocumentId = job.Key,
            IndexedAppId = appId,
            IndexedSchemaId = source.SchemaId.Id,
            IsDeleted = source.IsDeleted,
            LastModified = source.LastModified,
            LastModifiedBy = source.LastModifiedBy,
            NewData = null,
            NewStatus = null,
            ScheduledAt = null,
            ScheduleJob = null,
            SchemaId = source.SchemaId,
            Status = source.CurrentVersion.Status,
            TranslationStatus = translationStatus,
            Version = source.Version,
        };

        return (entity, references);
    }
}

public record EFContentEntity : Content, IVersionedEntity<DomainId>
{
    [Key]
    public DomainId DocumentId { get; set; }

    public DomainId IndexedAppId { get; set; }

    public DomainId IndexedSchemaId { get; set; }

    public Instant? ScheduledAt { get; set; }

    public ContentData? NewData { get; set; }

    public TranslationStatus? TranslationStatus { get; set; }

    public WriteContent ToState()
    {
        if (NewData != null && NewStatus.HasValue)
        {
            return new WriteContent
            {
                Id = Id,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                CurrentVersion = new ContentVersion(Status, NewData),
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                NewVersion = new ContentVersion(NewStatus.Value, Data),
                ScheduleJob = ScheduleJob,
                SchemaId = SchemaId,
                Version = Version,
            };
        }
        else
        {
            return new WriteContent
            {
                Id = Id,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                CurrentVersion = new ContentVersion(Status, Data),
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                NewVersion = null,
                ScheduleJob = ScheduleJob,
                SchemaId = SchemaId,
                Version = Version,
            };
        }
    }

    protected static async Task<(HashSet<DomainId>, TranslationStatus?)> CreateExtendedValuesAsync(
        WriteContent content,
        ContentData data,
        IAppProvider appProvider,
        CancellationToken ct)
    {
        var referencedIds = new HashSet<DomainId>();

        var (app, schema) = await appProvider.GetAppWithSchemaAsync(content.AppId.Id, content.SchemaId.Id, true, ct);

        if (app == null || schema == null)
        {
            return (referencedIds, null);
        }

        if (data.CanHaveReference())
        {
            var components = await appProvider.GetComponentsAsync(schema, ct: ct);

            data.AddReferencedIds(schema, referencedIds, components);
        }

        var translationStatus = TranslationStatus.Create(data, schema, app.Languages);

        return (referencedIds, translationStatus);
    }
}
