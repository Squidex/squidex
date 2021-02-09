// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : ISnapshotStore<ContentDomainObject.State, DomainId>
    {
        Task ISnapshotStore<ContentDomainObject.State, DomainId>.ReadAllAsync(Func<ContentDomainObject.State, long, Task> callback, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        async Task ISnapshotStore<ContentDomainObject.State, DomainId>.ClearAsync()
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.ClearAsync();
                await collectionPublished.ClearAsync();
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State, DomainId>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await collectionAll.RemoveAsync(key);
                await collectionPublished.RemoveAsync(key);
            }
        }

        async Task<(ContentDomainObject.State Value, long Version)> ISnapshotStore<ContentDomainObject.State, DomainId>.ReadAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                var contentEntity = await collectionAll.FindAsync(key);

                if (contentEntity != null)
                {
                    return (SimpleMapper.Map(contentEntity, new ContentDomainObject.State()), contentEntity.Version);
                }

                return (null!, EtagVersion.NotFound);
            }
        }

        async Task ISnapshotStore<ContentDomainObject.State, DomainId>.WriteAsync(DomainId key, ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (value.SchemaId.Id == DomainId.Empty)
                {
                    return;
                }

                await Task.WhenAll(
                    UpsertDraftContentAsync(value, oldVersion, newVersion),
                    UpsertOrDeletePublishedAsync(value, oldVersion, newVersion));
            }
        }

        private async Task UpsertOrDeletePublishedAsync(ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            if (value.Status == Status.Published && !value.IsDeleted)
            {
                await UpsertPublishedContentAsync(value, oldVersion, newVersion);
            }
            else
            {
                await DeletePublishedContentAsync(value.AppId.Id, value.Id);
            }
        }

        private Task DeletePublishedContentAsync(DomainId appId, DomainId id)
        {
            var documentId = DomainId.Combine(appId, id);

            return collectionPublished.RemoveAsync(documentId);
        }

        private async Task UpsertDraftContentAsync(ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            var content = await CreateContentAsync(value, value.Data, newVersion);

            content.ScheduledAt = value.ScheduleJob?.DueTime;
            content.ScheduleJob = value.ScheduleJob;
            content.NewStatus = value.NewStatus;

            await collectionAll.UpsertVersionedAsync(content.DocumentId, oldVersion, content);
        }

        private async Task UpsertPublishedContentAsync(ContentDomainObject.State value, long oldVersion, long newVersion)
        {
            var content = await CreateContentAsync(value, value.CurrentVersion.Data, newVersion);

            content.ScheduledAt = null;
            content.ScheduleJob = null;
            content.NewStatus = null;

            await collectionPublished.UpsertVersionedAsync(content.DocumentId, oldVersion, content);
        }

        private async Task<MongoContentEntity> CreateContentAsync(ContentDomainObject.State value, ContentData data, long newVersion)
        {
            var content = SimpleMapper.Map(value, new MongoContentEntity());

            content.Data = data;
            content.DocumentId = value.UniqueId;
            content.IndexedAppId = value.AppId.Id;
            content.IndexedSchemaId = value.SchemaId.Id;
            content.Version = newVersion;

            var schema = await appProvider.GetSchemaAsync(value.AppId.Id, value.SchemaId.Id, true);

            if (schema != null)
            {
                content.ReferencedIds = content.Data.GetReferencedIds(schema.SchemaDef);
            }

            return content;
        }
    }
}
