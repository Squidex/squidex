// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public record MongoContentEntity : Content, IVersionedEntity<DomainId>
{
    [BsonRequired]
    [BsonElement("_ai")]
    public DomainId IndexedAppId { get; set; }

    [BsonRequired]
    [BsonElement("_si")]
    public DomainId IndexedSchemaId { get; set; }

    [BsonRequired]
    [BsonElement("rf")]
    public HashSet<DomainId>? ReferencedIds { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("dd")]
    public ContentData? NewData { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("sa")]
    public Instant? ScheduledAt { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("is")]
    public bool IsSnapshot { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("ts")]
    public TranslationStatus? TranslationStatus { get; set; }

    public static void RegisterClassMap()
    {
        EntityClassMap.Register();

        BsonClassMap.TryRegisterClassMap<Content>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.Id)
                .SetElementName("id")
                .SetIgnoreIfDefault(true);

            cm.MapProperty(x => x.AppId)
                .SetElementName("ai")
                .SetIsRequired(true);

            cm.MapProperty(x => x.SchemaId)
                .SetElementName("si")
                .SetIsRequired(true);

            cm.MapProperty(x => x.IsDeleted)
                .SetElementName("dl")
                .SetIsRequired(true);

            cm.MapProperty(x => x.Data)
                .SetElementName("do")
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.NewStatus)
                .SetElementName("ns")
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.Status)
                .SetElementName("st")
                .SetIsRequired(true);

            cm.MapProperty(x => x.ScheduleJob)
                .SetElementName("sj")
                .SetIgnoreIfDefault(true);
        });
    }

    public WriteContent ToState()
    {
        if (NewData != null && NewStatus.HasValue)
        {
            return new WriteContent
            {
                UniqueId = UniqueId,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                CurrentVersion = new ContentVersion(Status, NewData),
                Id = Id,
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                NewVersion = new ContentVersion(NewStatus.Value, Data),
                ScheduleJob = ScheduleJob,
                SchemaId = SchemaId,
                Version = Version
            };
        }
        else
        {
            return new WriteContent
            {
                UniqueId = UniqueId,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                CurrentVersion = new ContentVersion(Status, Data),
                Id = Id,
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                NewVersion = null,
                ScheduleJob = ScheduleJob,
                SchemaId = SchemaId,
                Version = Version
            };
        }
    }

    public static async Task<MongoContentEntity> CreatePublishedAsync(SnapshotWriteJob<WriteContent> job, IAppProvider appProvider,
        CancellationToken ct)
    {
        var source = job.Value;

        var (referencedIds, translationStatus) = await CreateExtendedValuesAsync(source, source.CurrentVersion.Data, appProvider, ct);

        return new MongoContentEntity
        {
            UniqueId = source.UniqueId,
            IndexedAppId = source.AppId.Id,
            IndexedSchemaId = source.SchemaId.Id,
            AppId = source.AppId,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Data = source.EditingData,
            Id = source.Id,
            IsDeleted = source.IsDeleted,
            IsSnapshot = false,
            LastModified = source.LastModified,
            LastModifiedBy = source.LastModifiedBy,
            NewData = null,
            NewStatus = null,
            ReferencedIds = referencedIds,
            ScheduledAt = null,
            ScheduleJob = null,
            SchemaId = source.SchemaId,
            Status = source.CurrentVersion.Status,
            TranslationStatus = translationStatus,
            Version = source.Version,
        };
    }

    public static async Task<MongoContentEntity> CreateCompleteAsync(SnapshotWriteJob<WriteContent> job, IAppProvider appProvider,
        CancellationToken ct)
    {
        var source = job.Value;

        var (referencedIds, translationStatus) = await CreateExtendedValuesAsync(source, source.EditingData, appProvider, ct);

        return new MongoContentEntity
        {
            UniqueId = source.UniqueId,
            IndexedAppId = source.AppId.Id,
            IndexedSchemaId = source.SchemaId.Id,
            AppId = source.AppId,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Data = source.EditingData,
            Id = source.Id,
            IsDeleted = source.IsDeleted,
            IsSnapshot = false,
            LastModified = source.LastModified,
            LastModifiedBy = source.LastModifiedBy,
            NewData = source.NewVersion != null ? source.CurrentVersion.Data : null,
            NewStatus = source.NewVersion?.Status,
            ReferencedIds = referencedIds,
            ScheduledAt = source.ScheduleJob?.DueTime,
            ScheduleJob = source.ScheduleJob,
            SchemaId = source.SchemaId,
            Status = source.CurrentVersion.Status,
            TranslationStatus = translationStatus,
            Version = source.Version,
        };
    }

    private static async Task<(HashSet<DomainId>, TranslationStatus?)> CreateExtendedValuesAsync(WriteContent content, ContentData data, IAppProvider appProvider,
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
