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
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.OData;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;

#pragma warning disable RECS0147

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private static readonly Status[] StatusPublishedOnly = { Status.Published };
        private static readonly IResultList<IEnrichedContentEntity> EmptyContents = ResultList.CreateFrom<IEnrichedContentEntity>(0);
        private readonly IAppProvider appProvider;
        private readonly IAssetUrlGenerator assetUrlGenerator;
        private readonly IContentEnricher contentEnricher;
        private readonly IContentRepository contentRepository;
        private readonly IContentVersionLoader contentVersionLoader;
        private readonly IScriptEngine scriptEngine;
        private readonly ContentOptions options;
        private readonly EdmModelBuilder modelBuilder;

        public int DefaultPageSizeGraphQl
        {
            get { return options.DefaultPageSizeGraphQl; }
        }

        public ContentQueryService(
            IAppProvider appProvider,
            IAssetUrlGenerator assetUrlGenerator,
            IContentEnricher contentEnricher,
            IContentRepository contentRepository,
            IContentVersionLoader contentVersionLoader,
            IScriptEngine scriptEngine,
            IOptions<ContentOptions> options,
            EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(assetUrlGenerator, nameof(assetUrlGenerator));
            Guard.NotNull(contentEnricher, nameof(contentEnricher));
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(contentVersionLoader, nameof(contentVersionLoader));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.appProvider = appProvider;
            this.assetUrlGenerator = assetUrlGenerator;
            this.contentEnricher = contentEnricher;
            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.modelBuilder = modelBuilder;
            this.options = options.Value;
            this.scriptEngine = scriptEngine;
        }

        public async Task<IEnrichedContentEntity> FindContentAsync(Context context, string schemaIdOrName, Guid id, long version = -1)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            CheckPermission(context, schema);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                IContentEntity content;

                if (version > EtagVersion.Empty)
                {
                    content = await FindByVersionAsync(id, version);
                }
                else
                {
                    content = await FindCoreAsync(context, id, schema);
                }

                if (content == null || (content.Status != Status.Published && !context.IsFrontendClient) || content.SchemaId.Id != schema.Id)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
                }

                return await TransformAsync(context, schema, content);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q query)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            CheckPermission(context, schema);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                IResultList<IContentEntity> contents;

                if (query.Ids != null && query.Ids.Count > 0)
                {
                    contents = await QueryByIdsAsync(context, query, schema);
                }
                else
                {
                    contents = await QueryByQueryAsync(context, query, schema);
                }

                return await TransformAsync(context, schema, contents);
            }
        }

        public async Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, IReadOnlyList<Guid> ids)
        {
            Guard.NotNull(context, nameof(context));

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
                var results = new List<IEnrichedContentEntity>();

                var converters = GenerateConverters(context).ToArray();

                var scriptText = schema.SchemaDef.Scripts.Query;
                var scripting = !string.IsNullOrWhiteSpace(scriptText);

                var enriched = await contentEnricher.EnrichAsync(contents, context.User);

                foreach (var content in enriched)
                {
                    var result = SimpleMapper.Map(content, new ContentEntity());

                    if (result.Data != null)
                    {
                        if (!context.IsFrontendClient && scripting)
                        {
                            var ctx = new ScriptContext { User = context.User, Data = content.Data, ContentId = content.Id };

                            result.Data = scriptEngine.Transform(ctx, scriptText);
                        }

                        result.Data = result.Data.ConvertName2Name(schema.SchemaDef, converters);
                    }

                    if (result.DataDraft != null && (context.IsUnpublished() || context.IsFrontendClient))
                    {
                        result.DataDraft = result.DataDraft.ConvertName2Name(schema.SchemaDef, converters);
                    }
                    else
                    {
                        result.DataDraft = null;
                    }

                    results.Add(result);
                }

                return results;
            }
        }

        private IEnumerable<FieldConverter> GenerateConverters(Context context)
        {
            if (!context.IsFrontendClient)
            {
                yield return FieldConverters.ExcludeHidden();
                yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeHidden());
            }

            yield return FieldConverters.ExcludeChangedTypes();
            yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeChangedTypes());

            yield return FieldConverters.ResolveInvariant(context.App.LanguagesConfig);
            yield return FieldConverters.ResolveLanguages(context.App.LanguagesConfig);

            if (!context.IsFrontendClient)
            {
                yield return FieldConverters.ResolveFallbackLanguages(context.App.LanguagesConfig);

                var languages = context.Languages();

                if (languages.Any())
                {
                    yield return FieldConverters.FilterLanguages(context.App.LanguagesConfig, languages);
                }

                var assetUrls = context.AssetUrls();

                if (assetUrls.Any() == true)
                {
                    yield return FieldConverters.ResolveAssetUrls(assetUrls.ToList(), assetUrlGenerator);
                }
            }
        }

        private Query ParseQuery(Context context, string query, ISchemaEntity schema)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                try
                {
                    var model = modelBuilder.BuildEdmModel(context.App, schema, context.IsFrontendClient);

                    var result = model.ParseQuery(query).ToQuery();

                    if (result.Sort.Count == 0)
                    {
                        result.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
                    }

                    if (result.Take == long.MaxValue)
                    {
                        result.Take = options.DefaultPageSize;
                    }
                    else if (result.Take > options.MaxResults)
                    {
                        result.Take = options.MaxResults;
                    }

                    return result;
                }
                catch (NotSupportedException)
                {
                    throw new ValidationException("OData operation is not supported.");
                }
                catch (ODataException ex)
                {
                    throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
                }
            }
        }

        public async Task<ISchemaEntity> GetSchemaOrThrowAsync(Context context, string schemaIdOrName)
        {
            ISchemaEntity schema = null;

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

        private static Status[] GetStatus(Context context)
        {
            if (context.IsFrontendClient || context.IsUnpublished())
            {
                return null;
            }
            else
            {
                return StatusPublishedOnly;
            }
        }

        private async Task<IResultList<IContentEntity>> QueryByQueryAsync(Context context, Q query, ISchemaEntity schema)
        {
            var parsedQuery = ParseQuery(context, query.ODataQuery, schema);

            return await QueryCoreAsync(context, schema, parsedQuery);
        }

        private async Task<IResultList<IContentEntity>> QueryByIdsAsync(Context context, Q query, ISchemaEntity schema)
        {
            var contents = await QueryCoreAsync(context, schema, query.Ids.ToHashSet());

            return contents.SortSet(x => x.Id, query.Ids);
        }

        private Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryCoreAsync(Context context, IReadOnlyList<Guid> ids)
        {
            return contentRepository.QueryAsync(context.App, GetStatus(context), new HashSet<Guid>(ids), WithDraft(context));
        }

        private Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, ISchemaEntity schema, Query query)
        {
            return contentRepository.QueryAsync(context.App, schema, GetStatus(context), context.IsFrontendClient, query, WithDraft(context));
        }

        private Task<IResultList<IContentEntity>> QueryCoreAsync(Context context, ISchemaEntity schema, HashSet<Guid> ids)
        {
            return contentRepository.QueryAsync(context.App, schema, GetStatus(context), ids, WithDraft(context));
        }

        private Task<IContentEntity> FindCoreAsync(Context context, Guid id, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(context.App, schema, GetStatus(context), id, WithDraft(context));
        }

        private Task<IContentEntity> FindByVersionAsync(Guid id, long version)
        {
            return contentVersionLoader.LoadAsync(id, version);
        }

        private static bool WithDraft(Context context)
        {
            return context.IsUnpublished() || context.IsFrontendClient;
        }
    }
}
