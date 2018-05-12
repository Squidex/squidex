// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IInitializable
    {
        private readonly IMongoDatabase database;
        private readonly IAppProvider appProvider;
        private readonly MongoContentDraftCollection contentsDraft;
        private readonly MongoContentPublishedCollection contentsPublished;

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;

            contentsDraft = new MongoContentDraftCollection(database);
            contentsPublished = new MongoContentPublishedCollection(database);

            this.database = database;
        }

        public void Initialize()
        {
            contentsDraft.Initialize();
            contentsPublished.Initialize();
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            if (RequiresPublished(status))
            {
                return contentsPublished.QueryAsync(app, schema, odataQuery);
            }
            else
            {
                return contentsDraft.QueryAsync(app, schema, odataQuery, status, true);
            }
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            if (RequiresPublished(status))
            {
                return contentsPublished.QueryAsync(app, schema, ids);
            }
            else
            {
                return contentsDraft.QueryAsync(app, schema, ids, status);
            }
        }

        public Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Status[] status, Guid id)
        {
            if (RequiresPublished(status))
            {
                return contentsPublished.FindContentAsync(app, schema, id);
            }
            else
            {
                return contentsDraft.FindContentAsync(app, schema, id);
            }
        }

        public Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> ids)
        {
            return contentsDraft.QueryNotFoundAsync(appId, schemaId, ids);
        }

        public Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            return contentsDraft.QueryScheduledWithoutDataAsync(now, callback);
        }

        public Task ClearAsync()
        {
            return Task.WhenAll(
                contentsDraft.ClearAsync(),
                contentsPublished.ClearAsync());
        }

        public Task DeleteArchiveAsync()
        {
            return database.DropCollectionAsync("States_Contents_Archive");
        }

        private static bool RequiresPublished(Status[] status)
        {
            return status?.Length == 1 && status[0] == Status.Published;
        }
    }
}
