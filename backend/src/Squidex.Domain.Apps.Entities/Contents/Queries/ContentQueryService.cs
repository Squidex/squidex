// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;
using Squidex.Shared;

#pragma warning disable RECS0147

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private static readonly IResultList<IEnrichedContentEntity> EmptyContents = ResultList.CreateFrom<IEnrichedContentEntity>(0);
        private readonly IAppProvider appProvider;
        private readonly IContentEnricher contentEnricher;
        private readonly IContentRepository contentRepository;
        private readonly IContentLoader contentVersionLoader;
        private readonly ContentQueryParser queryParser;

        public ContentQueryService(
            IAppProvider appProvider,
            IContentEnricher contentEnricher,
            IContentRepository contentRepository,
            IContentLoader contentVersionLoader,
            ContentQueryParser queryParser)
        {
            Guard.NotNull(appProvider);
            Guard.NotNull(contentEnricher);
            Guard.NotNull(contentRepository);
            Guard.NotNull(contentVersionLoader);
            Guard.NotNull(queryParser);

            this.appProvider = appProvider;
            this.contentEnricher = contentEnricher;
            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.queryParser = queryParser;
            this.queryParser = queryParser;
        }

        public async Task<IEnrichedContentEntity> FindContentAsync(Context context, string schemaIdOrName, Guid id, long version = -1)
        {
            Guard.NotNull(context);

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            CheckPermission(context, schema);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                IContentEntity? content;

                if (version > EtagVersion.Empty)
                {
                    content = await FindByVersionAsync(id, version);
                }
                else
                {
                    content = await FindCoreAsync(context, id, schema);
                }

                if (content == null || content.SchemaId.Id != schema.Id)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
                }

                return await TransformAsync(context, content);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q query)
        {
            Guard.NotNull(context);

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            CheckPermission(context, schema);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                IResultList<IContentEntity> contents;

                if (query.Ids != null && query.Ids.Count > 0)
                {
                    contents = await QueryByIdsAsync(context, schema, query);
                }
                else
                {
                    contents = await QueryByQueryAsync(context, schema, query);
                }

                return await TransformAsync(context, contents);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, IReadOnlyList<Guid> ids)
        {
            Guard.NotNull(context);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                if (ids == null || ids.Count == 0)
                {
                    return EmptyContents;
                }

                var contents = await QueryCoreAsync(context, ids);

                var filtered =
                    contents
                        .GroupBy(x => x.Schema.Id)
                        .Select(g => FilterContents(g, context))
                        .SelectMany(c => c);

                var results = await TransformCoreAsync(context, filtered);

                return ResultList.Create(results.Count, results.SortList(x => x.Id, ids));
            }
        }

        private async Task<IResultList<IEnrichedContentEntity>> TransformAsync(Context context, IResultList<IContentEntity> contents)
        {
            var transformed = await TransformCoreAsync(context, contents);

            return ResultList.Create(contents.Total, transformed);
        }

        private async Task<IEnrichedContentEntity> TransformAsync(Context context, IContentEntity content)
        {
            var transformed = await TransformCoreAsync(context, Enumerable.Repeat(content, 1));

            return transformed[0];
        }

        private async Task<IReadOnlyList<IEnrichedContentEntity>> TransformCoreAsync(Context context, IEnumerable<IContentEntity> contents)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                return await contentEnricher.EnrichAsync(contents, context);
            }
        }

        public async Task<ISchemaEntity> GetSchemaOrThrowAsync(Context context, string schemaIdOrName)
        {
            ISchemaEntity? schema = null;

            if (Guid.TryParse(schemaIdOrName, out var id))
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, id);
            }

            if (schema == null)
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, schemaIdOrName);
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaIdOrName, typeof(ISchemaEntity));
            }

            return schema;
        }

        private static void CheckPermission(Context context, params ISchemaEntity[] schemas)
        {
            foreach (var schema in schemas)
            {
                if (!HasPermission(context, schema))
                {
                    throw new DomainForbiddenException("You do not have permission for this schema.");
                }
            }
        }

        private static IEnumerable<IContentEntity> FilterContents(IGrouping<Guid, (IContentEntity Content, ISchemaEntity Schema)> group, Context context)
        {
            var schema = group.First().Schema;

            if (HasPermission(context, schema))
            {
                return group.Select(x => x.Content);
            }
            else
            {
                return Enumerable.Empty<IContentEntity>();
            }
        }

        private static bool HasPermission(Context context, ISchemaEntity schema)
        {
            var permission = Permissions.ForApp(Permissions.AppContentsRead, schema.AppId.Name, schema.SchemaDef.Name);

            return context.Permissions.Allows(permission);
        }

        private async Task<IResultList<IContentEntity>> QueryByQueryAsync(Context context, ISchemaEntity schema, Q query)
        {
            var parsedQuery = queryParser.ParseQuery(context, schema, query);

            return await QueryCoreAsync(context, schema, parsedQuery);
        }

        private async Task<IResultList<IContentEntity>> QueryByIdsAsync(Context context, ISchemaEntity schema, Q query)
        {
            var contents = await QueryCoreAsync(context, schema, query.Ids.ToHashSet());

            return contents.SortSet(x => x.Id, query.Ids);
        }

        private Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryCoreAsync(Context context, IReadOnlyList<Guid> ids)
        {
            return contentRepository.QueryAsync(context.App, new HashSet<Guid>(ids), context.Scope());
        }

        private Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, ISchemaEntity schema, ClrQuery query)
        {
            return contentRepository.QueryAsync(context.App, schema, query, context.Scope());
        }

        private Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, ISchemaEntity schema, HashSet<Guid> ids)
        {
            return contentRepository.QueryAsync(context.App, schema, ids, context.Scope());
        }

        private Task<IContentEntity?> FindCoreAsync(Context context, Guid id, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(context.App, schema, id, context.Scope());
        }

        private Task<IContentEntity> FindByVersionAsync(Guid id, long version)
        {
            return contentVersionLoader.GetAsync(id, version);
        }
    }
}
