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
using Squidex.Infrastructure.Translations;
using Squidex.Log;
using Squidex.Shared;

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
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(contentEnricher, nameof(contentEnricher));
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(contentVersionLoader, nameof(contentVersionLoader));
            Guard.NotNull(queryParser, nameof(queryParser));

            this.appProvider = appProvider;
            this.contentEnricher = contentEnricher;
            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.queryParser = queryParser;
            this.queryParser = queryParser;
        }

        public async Task<IEnrichedContentEntity> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = -1)
        {
            Guard.NotNull(context, nameof(context));

            if (id == default)
            {
                throw new DomainObjectNotFoundException(id.ToString());
            }

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                IContentEntity? content;

                if (version > EtagVersion.Empty)
                {
                    content = await FindByVersionAsync(context, id, version);
                }
                else
                {
                    content = await FindCoreAsync(context, id, schema);
                }

                if (content == null || content.SchemaId.Id != schema.Id)
                {
                    throw new DomainObjectNotFoundException(id.ToString());
                }

                return await TransformAsync(context, content);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q q)
        {
            Guard.NotNull(context, nameof(context));

            if (q == null)
            {
                return EmptyContents;
            }

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                q = await queryParser.ParseAsync(context, q, schema);

                var contents = await contentRepository.QueryAsync(context.App, schema, q, context.Scope());

                if (q.Ids != null)
                {
                    contents = contents.SortSet(x => x.Id, q.Ids);
                }

                return await TransformAsync(context, contents);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, Q q)
        {
            Guard.NotNull(context, nameof(context));

            if (q == null)
            {
                return EmptyContents;
            }

            var schemas = await GetSchemasAsync(context);

            if (schemas.Count == 0)
            {
                return EmptyContents;
            }

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                q = await queryParser.ParseAsync(context, q);

                var contents = await contentRepository.QueryAsync(context.App, schemas, q, context.Scope());

                if (q.Ids != null)
                {
                    contents = contents.SortSet(x => x.Id, q.Ids);
                }

                return await TransformAsync(context, contents);
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

            var canCache = !context.IsFrontendClient;

            if (Guid.TryParse(schemaIdOrName, out var guid))
            {
                var schemaId = DomainId.Create(guid);

                schema = await appProvider.GetSchemaAsync(context.App.Id, schemaId, false, canCache);
            }

            if (schema == null)
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, schemaIdOrName, canCache);
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaIdOrName);
            }

            if (!HasPermission(context, schema))
            {
                throw new DomainForbiddenException(T.Get("schemas.noPermission"));
            }

            return schema;
        }

        private async Task<List<ISchemaEntity>> GetSchemasAsync(Context context)
        {
            var schemas = await appProvider.GetSchemasAsync(context.App.Id);

            return schemas.Where(x => HasPermission(context, x)).ToList();
        }

        private static bool HasPermission(Context context, ISchemaEntity schema)
        {
            var permission = Permissions.ForApp(Permissions.AppContentsRead, context.App.Name, schema.SchemaDef.Name);

            return context.Permissions.Allows(permission);
        }

        private Task<IContentEntity?> FindCoreAsync(Context context, DomainId id, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(context.App, schema, id, context.Scope());
        }

        private Task<IContentEntity> FindByVersionAsync(Context context, DomainId id, long version)
        {
            return contentVersionLoader.GetAsync(context.App.Id, id, version);
        }
    }
}
