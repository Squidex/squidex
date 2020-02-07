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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;

#pragma warning disable RECS0147

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private static readonly Status[] StatusPublishedOnly = { Status.Published };
        private static readonly IResultList<IEnrichedContentEntity> EmptyContents = ResultList.CreateFrom<IEnrichedContentEntity>(0);
        private readonly IAppProvider appProvider;
        private readonly IContentEnricher contentEnricher;
        private readonly IContentRepository contentRepository;
        private readonly IContentLoader contentVersionLoader;
        private readonly IScriptEngine scriptEngine;
        private readonly ContentQueryParser queryParser;

        public ContentQueryService(
            IAppProvider appProvider,
            IContentEnricher contentEnricher,
            IContentRepository contentRepository,
            IContentLoader contentVersionLoader,
            IScriptEngine scriptEngine,
            ContentQueryParser queryParser)
        {
            Guard.NotNull(appProvider);
            Guard.NotNull(contentEnricher);
            Guard.NotNull(contentRepository);
            Guard.NotNull(contentVersionLoader);
            Guard.NotNull(queryParser);
            Guard.NotNull(scriptEngine);

            this.appProvider = appProvider;
            this.contentEnricher = contentEnricher;
            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.queryParser = queryParser;
            this.scriptEngine = scriptEngine;
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

                return await TransformAsync(context, schema, content);
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

                return await TransformAsync(context, schema, contents);
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

                var results = new List<IEnrichedContentEntity>();

                var contents = await QueryCoreAsync(context, ids);

                foreach (var group in contents.GroupBy(x => x.Schema.Id))
                {
                    var schema = group.First().Schema;

                    if (HasPermission(context, schema))
                    {
                        var enriched = await TransformCoreAsync(context, schema, group.Select(x => x.Content));

                        results.AddRange(enriched);
                    }
                }

                return ResultList.Create(results.Count, results.SortList(x => x.Id, ids));
            }
        }

        private async Task<IResultList<IEnrichedContentEntity>> TransformAsync(Context context, ISchemaEntity schema, IResultList<IContentEntity> contents)
        {
            var transformed = await TransformCoreAsync(context, schema, contents);

            return ResultList.Create(contents.Total, transformed);
        }

        private async Task<IEnrichedContentEntity> TransformAsync(Context context, ISchemaEntity schema, IContentEntity content)
        {
            var transformed = await TransformCoreAsync(context, schema, Enumerable.Repeat(content, 1));

            return transformed[0];
        }

        private async Task<IReadOnlyList<IEnrichedContentEntity>> TransformCoreAsync(Context context, ISchemaEntity schema, IEnumerable<IContentEntity> contents)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var enriched = await contentEnricher.EnrichAsync(contents, context);

                var script = schema.SchemaDef.Scripts.Query;

                if (!string.IsNullOrWhiteSpace(script) && !context.IsFrontendClient)
                {
                    var results = new List<IEnrichedContentEntity>();

                    var scriptContext = new ScriptContext { User = context.User };

                    foreach (var content in enriched)
                    {
                        scriptContext.Data = content.Data;
                        scriptContext.ContentId = content.Id;

                        var result = SimpleMapper.Map(content, new ContentEntity());

                        result.Data = scriptEngine.Transform(scriptContext, script);

                        results.Add(result);
                    }

                    return results;
                }
                else
                {
                    return enriched;
                }
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

        private static bool HasPermission(Context context, ISchemaEntity schema)
        {
            var permission = Permissions.ForApp(Permissions.AppContentsRead, schema.AppId.Name, schema.SchemaDef.Name);

            return context.Permissions.Allows(permission);
        }

        private static Status[]? GetStatus(Context context)
        {
            if (context.IsFrontendClient || context.ShouldProvideUnpublished())
            {
                return null;
            }
            else
            {
                return StatusPublishedOnly;
            }
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
            return contentRepository.QueryAsync(context.App, GetStatus(context), new HashSet<Guid>(ids), context.Scope());
        }

        private Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, ISchemaEntity schema, ClrQuery query)
        {
            return contentRepository.QueryAsync(context.App, schema, GetStatus(context), query, context.Scope());
        }

        private Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, ISchemaEntity schema, HashSet<Guid> ids)
        {
            return contentRepository.QueryAsync(context.App, schema, GetStatus(context), ids, context.Scope());
        }

        private Task<IContentEntity?> FindCoreAsync(Context context, Guid id, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(context.App, schema, GetStatus(context), id, context.Scope());
        }

        private Task<IContentEntity> FindByVersionAsync(Guid id, long version)
        {
            return contentVersionLoader.GetAsync(id, version);
        }
    }
}
