﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Log;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private static readonly IResultList<IEnrichedContentEntity> EmptyContents = ResultList.CreateFrom<IEnrichedContentEntity>(0);
        private readonly IAppProvider appProvider;
        private readonly IContentEnricher contentEnricher;
        private readonly IContentRepository contentRepository;
        private readonly IContentLoader contentLoader;
        private readonly ContentQueryParser queryParser;
        private readonly ContentOptions options;

        public ContentQueryService(
            IAppProvider appProvider,
            IContentEnricher contentEnricher,
            IContentRepository contentRepository,
            IContentLoader contentLoader,
            IOptions<ContentOptions> options,
            ContentQueryParser queryParser)
        {
            this.appProvider = appProvider;
            this.contentEnricher = contentEnricher;
            this.contentRepository = contentRepository;
            this.contentLoader = contentLoader;
            this.options = options.Value;
            this.queryParser = queryParser;
        }

        public async Task<IEnrichedContentEntity?> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

                IContentEntity? content;

                if (version > EtagVersion.Empty)
                {
                    content = await contentLoader.GetAsync(context.App.Id, id, version);
                }
                else
                {
                    content = await FindCoreAsync(context, id, schema, ct);
                }

                if (content == null || content.SchemaId.Id != schema.Id)
                {
                    return null;
                }

                return await TransformAsync(context, content, ct);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q q,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                if (q == null)
                {
                    return EmptyContents;
                }

                var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

                if (!HasPermission(context, schema, Permissions.AppContentsRead))
                {
                    q = q with { CreatedBy = context.User.Token() };
                }

                q = await queryParser.ParseAsync(context, q, schema);

                var contents = await QueryCoreAsync(context, q, schema, ct);

                if (q.Ids != null && q.Ids.Count > 0)
                {
                    contents = contents.SortSet(x => x.Id, q.Ids);
                }

                return await TransformAsync(context, contents, ct);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, Q q,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                if (q == null)
                {
                    return EmptyContents;
                }

                var schemas = await GetSchemasAsync(context);

                if (schemas.Count == 0)
                {
                    return EmptyContents;
                }

                q = await queryParser.ParseAsync(context, q);

                var contents = await QueryCoreAsync(context, q, schemas, ct);

                if (q.Ids != null && q.Ids.Count > 0)
                {
                    contents = contents.SortSet(x => x.Id, q.Ids);
                }

                return await TransformAsync(context, contents, ct);
            }
        }

        private async Task<IResultList<IEnrichedContentEntity>> TransformAsync(Context context, IResultList<IContentEntity> contents,
            CancellationToken ct)
        {
            var transformed = await TransformCoreAsync(context, contents, ct);

            return ResultList.Create(contents.Total, transformed);
        }

        private async Task<IEnrichedContentEntity> TransformAsync(Context context, IContentEntity content,
            CancellationToken ct)
        {
            var transformed = await TransformCoreAsync(context, Enumerable.Repeat(content, 1), ct);

            return transformed[0];
        }

        private async Task<IReadOnlyList<IEnrichedContentEntity>> TransformCoreAsync(Context context, IEnumerable<IContentEntity> contents,
            CancellationToken ct)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                return await contentEnricher.EnrichAsync(contents, context, ct);
            }
        }

        public async Task<ISchemaEntity> GetSchemaOrThrowAsync(Context context, string schemaIdOrName)
        {
            var schema = await GetSchemaAsync(context, schemaIdOrName);

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaIdOrName);
            }

            return schema;
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(Context context, string schemaIdOrName)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            ISchemaEntity? schema = null;

            var canCache = !context.IsFrontendClient;

            if (Guid.TryParse(schemaIdOrName, out var guid))
            {
                var schemaId = DomainId.Create(guid);

                schema = await appProvider.GetSchemaAsync(context.App.Id, schemaId, canCache);
            }

            if (schema == null)
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, schemaIdOrName, canCache);
            }

            if (schema != null && !HasPermission(context, schema, Permissions.AppContentsReadOwn))
            {
                throw new DomainForbiddenException(T.Get("schemas.noPermission"));
            }

            return schema;
        }

        private async Task<List<ISchemaEntity>> GetSchemasAsync(Context context)
        {
            var schemas = await appProvider.GetSchemasAsync(context.App.Id);

            return schemas.Where(x => IsAccessible(x) && HasPermission(context, x, Permissions.AppContentsReadOwn)).ToList();
        }

        private async Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, Q q, ISchemaEntity schema,
            CancellationToken ct)
        {
            using (var timeout = new CancellationTokenSource(options.TimeoutQuery))
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, ct))
                {
                    return await contentRepository.QueryAsync(context.App, schema, q, context.Scope(), ct);
                }
            }
        }

        private async Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, Q q, List<ISchemaEntity> schemas,
            CancellationToken ct)
        {
            using (var timeout = new CancellationTokenSource(options.TimeoutQuery))
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, ct))
                {
                    return await contentRepository.QueryAsync(context.App, schemas, q, context.Scope(), ct);
                }
            }
        }

        private async Task<IContentEntity?> FindCoreAsync(Context context, DomainId id, ISchemaEntity schema,
            CancellationToken ct)
        {
            using (var timeout = new CancellationTokenSource(options.TimeoutFind))
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, ct))
                {
                    return await contentRepository.FindContentAsync(context.App, schema, id, context.Scope(), combined.Token);
                }
            }
        }

        private static bool IsAccessible(ISchemaEntity schema)
        {
            return schema.SchemaDef.IsPublished;
        }

        private static bool HasPermission(Context context, ISchemaEntity schema, string permissionId)
        {
            return context.UserPermissions.Allows(permissionId, context.App.Name, schema.SchemaDef.Name);
        }
    }
}
