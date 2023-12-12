// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public sealed class ContentQueryService : IContentQueryService
{
    private const string SingletonId = "_schemaId_";
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

    public async IAsyncEnumerable<EnrichedContent> StreamAsync(Context context, string schemaIdOrName, int skip,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        Guard.NotNull(context);

        // We assume that the user has the full read permissions for this schema to optimize the DB query.
        var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName, ct);

        // Skip all expensive operations when we call the enricher.
        context = context.Clone(b => b
            .WithNoScripting()
            .WithNoCacheKeys()
            .WithNoEnrichment());

        // We run this query without a timeout because it is meant for long running background operations.
        var contents = contentRepository.StreamAll(context.App.Id, HashSet.Of(schema.Id), context.Scope(), ct);

        await foreach (var content in contents.WithCancellation(ct))
        {
            yield return await contentEnricher.EnrichAsync(content, false, context, ct);
        }
    }

    public async Task<EnrichedContent?> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        using (var activity = Telemetry.Activities.StartActivity("ContentQueryService/FindAsync"))
        {
            activity?.SetTag("schemaName", schemaIdOrName);
            activity?.SetTag("contentId", id);

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName, ct);

            Content? content;

            // A special ID to always query the single content of the singleton.
            if (id.ToString().Equals(SingletonId, StringComparison.Ordinal))
            {
                id = schema.Id;
            }

            if (version > EtagVersion.Empty)
            {
                content = await contentLoader.GetAsync(context.App.Id, id, version, ct);
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

    public async Task<IResultList<EnrichedContent>> QueryAsync(Context context, string schemaIdOrName, Q q,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        using (var activity = Telemetry.Activities.StartActivity("ContentQueryService/QueryAsync"))
        {
            activity?.SetTag("schemaName", schemaIdOrName);

            // Usually the query should not be null, but we never know.
            if (q == null)
            {
                return ResultList.Empty<EnrichedContent>();
            }

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName, ct);

            // The API only checks for read.own permission, so we might need an additional filter here.
            if (!HasPermission(context, schema, PermissionIds.AppContentsRead))
            {
                q = q with { CreatedBy = context.UserPrincipal.Token() };
            }

            q = await ParseCoreAsync(context, q, schema, ct);

            var contents = await QueryCoreAsync(context, q, schema, ct);

            if (q.Ids is { Count: > 0 })
            {
                contents = contents.Sorted(x => x.Id, q.Ids);
            }

            return await TransformAsync(context, contents, ct);
        }
    }

    public async Task<IResultList<EnrichedContent>> QueryAsync(Context context, Q q,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);

        using (Telemetry.Activities.StartActivity("ContentQueryService/QueryAsync"))
        {
            // Usually the query should not be null, but we never know.
            if (q == null)
            {
                return ResultList.Empty<EnrichedContent>();
            }

            var schemas = await GetSchemasAsync(context, ct);

            // If the user does not have a permission to query a single schema the database would return an empty result anyway.
            if (schemas.Count == 0)
            {
                return ResultList.Empty<EnrichedContent>();
            }

            q = await ParseCoreAsync(context, q, null, ct);

            var contents = await QueryCoreAsync(context, q, schemas, ct);

            if (q.Ids is { Count: > 0 })
            {
                contents = contents.Sorted(x => x.Id, q.Ids);
            }

            return await TransformAsync(context, contents, ct);
        }
    }

    private async Task<IResultList<EnrichedContent>> TransformAsync(Context context, IResultList<Content> contents,
        CancellationToken ct)
    {
        var transformed = await TransformCoreAsync(context, contents, ct);

        return ResultList.Create(contents.Total, transformed);
    }

    private async Task<EnrichedContent> TransformAsync(Context context, Content content,
        CancellationToken ct)
    {
        var transformed = await TransformCoreAsync(context, Enumerable.Repeat(content, 1), ct);

        return transformed[0];
    }

    private async Task<IReadOnlyList<EnrichedContent>> TransformCoreAsync(Context context, IEnumerable<Content> contents,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("ContentQueryService/TransformCoreAsync"))
        {
            return await contentEnricher.EnrichAsync(contents, context, ct);
        }
    }

    public async Task<Schema> GetSchemaOrThrowAsync(Context context, string schemaIdOrName,
        CancellationToken ct = default)
    {
        var schema = await GetSchemaAsync(context, schemaIdOrName, ct);

        if (schema == null)
        {
            throw new DomainObjectNotFoundException(schemaIdOrName);
        }

        return schema;
    }

    public async Task<Schema?> GetSchemaAsync(Context context, string schemaIdOrName,
        CancellationToken ct = default)
    {
        Guard.NotNull(context);
        Guard.NotNullOrEmpty(schemaIdOrName);

        Schema? schema = null;

        var canCache = !context.IsFrontendClient;

        if (Guid.TryParseExact(schemaIdOrName, "D", out var guid))
        {
            var schemaId = DomainId.Create(guid);

            schema = await appProvider.GetSchemaAsync(context.App.Id, schemaId, canCache, ct);
        }

        schema ??= await appProvider.GetSchemaAsync(context.App.Id, schemaIdOrName, canCache, ct);

        if (schema != null && !HasPermission(context, schema, PermissionIds.AppContentsReadOwn))
        {
            throw new DomainForbiddenException(T.Get("schemas.noPermission"));
        }

        return schema;
    }

    private async Task<List<Schema>> GetSchemasAsync(Context context,
        CancellationToken ct)
    {
        var schemas = await appProvider.GetSchemasAsync(context.App.Id, ct);

        return schemas.Where(x => IsAccessible(x) && HasPermission(context, x, PermissionIds.AppContentsReadOwn)).ToList();
    }

    private async Task<Q> ParseCoreAsync(Context context, Q q, Schema? schema,
        CancellationToken ct)
    {
        using (var combined = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            // Enforce a hard timeout
            combined.CancelAfter(options.TimeoutQuery);

            return await queryParser.ParseAsync(context, q, schema, ct);
        }
    }

    private async Task<IResultList<Content>> QueryCoreAsync(Context context, Q q, Schema schema,
        CancellationToken ct)
    {
        using (var combined = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            // Enforce a hard timeout
            combined.CancelAfter(options.TimeoutQuery);

            return await contentRepository.QueryAsync(context.App, schema, q, context.Scope(), combined.Token);
        }
    }

    private async Task<IResultList<Content>> QueryCoreAsync(Context context, Q q, List<Schema> schemas,
        CancellationToken ct)
    {
        using (var combined = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            // Enforce a hard timeout
            combined.CancelAfter(options.TimeoutQuery);

            return await contentRepository.QueryAsync(context.App, schemas, q, context.Scope(), combined.Token);
        }
    }

    private async Task<Content?> FindCoreAsync(Context context, DomainId id, Schema schema,
        CancellationToken ct)
    {
        using (var combined = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            // Enforce a hard timeout
            combined.CancelAfter(options.TimeoutFind);

            return await contentRepository.FindContentAsync(context.App, schema, id, context.Scope(), combined.Token);
        }
    }

    private static bool IsAccessible(Schema schema)
    {
        return schema.IsPublished;
    }

    private static bool HasPermission(Context context, Schema schema, string permissionId)
    {
        return context.UserPermissions.Allows(permissionId, context.App.Name, schema.Name);
    }
}
