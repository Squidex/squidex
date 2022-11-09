// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public sealed class MongoContentEntity : IContentEntity, IVersionedEntity<DomainId>
{
    [BsonId]
    [BsonElement("_id")]
    public DomainId DocumentId { get; set; }

    [BsonRequired]
    [BsonElement("_ai")]
    public DomainId IndexedAppId { get; set; }

    [BsonRequired]
    [BsonElement("_si")]
    public DomainId IndexedSchemaId { get; set; }

    [BsonRequired]
    [BsonElement("ai")]
    public NamedId<DomainId> AppId { get; set; }

    [BsonRequired]
    [BsonElement("si")]
    public NamedId<DomainId> SchemaId { get; set; }

    [BsonRequired]
    [BsonElement("rf")]
    public HashSet<DomainId>? ReferencedIds { get; set; }

    [BsonRequired]
    [BsonElement("id")]
    public DomainId Id { get; set; }

    [BsonRequired]
    [BsonElement("ss")]
    public Status Status { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("ns")]
    public Status? NewStatus { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("do")]
    public ContentData Data { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("dd")]
    public ContentData? DraftData { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("sa")]
    public Instant? ScheduledAt { get; set; }

    [BsonRequired]
    [BsonElement("ct")]
    public Instant Created { get; set; }

    [BsonRequired]
    [BsonElement("mt")]
    public Instant LastModified { get; set; }

    [BsonRequired]
    [BsonElement("vs")]
    public long Version { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("dl")]
    public bool IsDeleted { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("is")]
    public bool IsSnapshot { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("sj")]
    public ScheduleJob? ScheduleJob { get; set; }

    [BsonRequired]
    [BsonElement("cb")]
    public RefToken CreatedBy { get; set; }

    [BsonRequired]
    [BsonElement("mb")]
    public RefToken LastModifiedBy { get; set; }

    [BsonIgnoreIfNull]
    [BsonElement("ts")]
    public TranslationStatus? TranslationStatus { get; set; }

    public DomainId UniqueId
    {
        get => DocumentId;
    }

    public ContentDomainObject.State ToState()
    {
        var state = SimpleMapper.Map(this, new ContentDomainObject.State());

        if (DraftData != null && NewStatus.HasValue)
        {
            state.NewVersion = new ContentVersion(NewStatus.Value, Data);
            state.CurrentVersion = new ContentVersion(Status, DraftData);
        }
        else
        {
            state.NewVersion = null;
            state.CurrentVersion = new ContentVersion(Status, Data);
        }

        return state;
    }

    public static async Task<MongoContentEntity> CreatePublishedAsync(SnapshotWriteJob<ContentDomainObject.State> job, IAppProvider appProvider)
    {
        var entity = await CreateContentAsync(job.Value.CurrentVersion.Data, job, appProvider);

        entity.ScheduledAt = null;
        entity.ScheduleJob = null;
        entity.NewStatus = null;

        return entity;
    }

    public static async Task<MongoContentEntity> CreateCompleteAsync(SnapshotWriteJob<ContentDomainObject.State> job, IAppProvider appProvider)
    {
        var entity = await CreateContentAsync(job.Value.Data, job, appProvider);

        entity.ScheduledAt = job.Value.ScheduleJob?.DueTime;
        entity.ScheduleJob = job.Value.ScheduleJob;
        entity.NewStatus = job.Value.NewStatus;
        entity.DraftData = job.Value.NewVersion != null ? job.Value.CurrentVersion.Data : null;
        entity.IsSnapshot = true;

        return entity;
    }

    private static async Task<MongoContentEntity> CreateContentAsync(ContentData data, SnapshotWriteJob<ContentDomainObject.State> job, IAppProvider appProvider)
    {
        var entity = SimpleMapper.Map(job.Value, new MongoContentEntity());

        entity.Data = data;
        entity.DocumentId = job.Value.UniqueId;
        entity.IndexedAppId = job.Value.AppId.Id;
        entity.IndexedSchemaId = job.Value.SchemaId.Id;
        entity.ReferencedIds ??= new HashSet<DomainId>();
        entity.Version = job.NewVersion;

        var (app, schema) = await appProvider.GetAppWithSchemaAsync(job.Value.AppId.Id, job.Value.SchemaId.Id, true);

        if (schema?.SchemaDef != null && app != null)
        {
            if (data.CanHaveReference())
            {
                var components = await appProvider.GetComponentsAsync(schema);

                entity.Data.AddReferencedIds(schema.SchemaDef, entity.ReferencedIds, components);
            }

            entity.TranslationStatus = TranslationStatus.Create(data, schema.SchemaDef, app.Languages);
        }

        return entity;
    }
}
