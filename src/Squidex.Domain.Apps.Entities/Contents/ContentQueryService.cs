// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

#pragma warning disable RECS0147

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private static readonly Status[] StatusPublishedOnly = { Status.Published };
        private readonly IContentRepository contentRepository;
        private readonly IContentVersionLoader contentVersionLoader;
        private readonly IAppProvider appProvider;
        private readonly IAssetUrlGenerator assetUrlGenerator;
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
            IContentRepository contentRepository,
            IContentVersionLoader contentVersionLoader,
            IScriptEngine scriptEngine,
            IOptions<ContentOptions> options,
            EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(assetUrlGenerator, nameof(assetUrlGenerator));
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(contentVersionLoader, nameof(contentVersionLoader));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.appProvider = appProvider;
            this.assetUrlGenerator = assetUrlGenerator;
            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.modelBuilder = modelBuilder;
            this.options = options.Value;
            this.scriptEngine = scriptEngine;
        }

        public async Task<IContentEntity> FindContentAsync(QueryContext context, string schemaIdOrName, Guid id, long version = -1)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            CheckPermission(context.User, schema);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var isVersioned = version > EtagVersion.Empty;

                var status = GetStatus(context);

                var content =
                    isVersioned ?
                    await FindContentByVersionAsync(id, version) :
                    await FindContentAsync(context, id, status, schema);

                if (content == null || (content.Status != Status.Published && !context.IsFrontendClient) || content.SchemaId.Id != schema.Id)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
                }

                return Transform(context, schema, content);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, string schemaIdOrName, Q query)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaOrThrowAsync(context, schemaIdOrName);

            CheckPermission(context.User, schema);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var status = GetStatus(context);

                IResultList<IContentEntity> contents;

                if (query.Ids?.Count > 0)
                {
                    contents = await QueryAsync(context, schema, query.Ids.ToHashSet(), status);
                    contents = SortSet(contents, query.Ids);
                }
                else
                {
                    var parsedQuery = ParseQuery(context, query.ODataQuery, schema);

                    contents = await QueryAsync(context, schema, parsedQuery, status);
                }

                return Transform(context, schema, contents);
            }
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(QueryContext context, IReadOnlyList<Guid> ids)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var status = GetStatus(context);

                List<IContentEntity> result;

                if (ids?.Count > 0)
                {
                    var contents = await QueryAsync(context, ids, status);

                    var permissions = context.User.Permissions();

                    contents = contents.Where(x => HasPermission(permissions, x.Schema)).ToList();

                    result = contents.Select(x => Transform(context, x.Schema, x.Content)).ToList();
                    result = SortList(result, ids).ToList();
                }
                else
                {
                    result = new List<IContentEntity>();
                }

                return result;
            }
        }

        private IResultList<IContentEntity> Transform(QueryContext context, ISchemaEntity schema, IResultList<IContentEntity> contents)
        {
            var transformed = TransformCore(context, schema, contents);

            return ResultList.Create(contents.Total, transformed);
        }

        private IContentEntity Transform(QueryContext context, ISchemaEntity schema, IContentEntity content)
        {
            return TransformCore(context, schema, Enumerable.Repeat(content, 1)).FirstOrDefault();
        }

        private static IResultList<IContentEntity> SortSet(IResultList<IContentEntity> contents, IReadOnlyList<Guid> ids)
        {
            return ResultList.Create(contents.Total, SortList(contents, ids));
        }

        private static IEnumerable<IContentEntity> SortList(IEnumerable<IContentEntity> contents, IReadOnlyList<Guid> ids)
        {
            return ids.Select(id => contents.FirstOrDefault(x => x.Id == id)).Where(x => x != null);
        }

        private IEnumerable<IContentEntity> TransformCore(QueryContext context, ISchemaEntity schema, IEnumerable<IContentEntity> contents)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var converters = GenerateConverters(context).ToArray();

                var scriptText = schema.SchemaDef.Scripts.Query;

                var isScripting = !string.IsNullOrWhiteSpace(scriptText);

                foreach (var content in contents)
                {
                    var result = SimpleMapper.Map(content, new ContentEntity());

                    if (result.Data != null)
                    {
                        if (!context.IsFrontendClient && isScripting)
                        {
                            var ctx = new ScriptContext { User = context.User, Data = content.Data, ContentId = content.Id };

                            result.Data = scriptEngine.Transform(ctx, scriptText);
                        }

                        result.Data = result.Data.ConvertName2Name(schema.SchemaDef, converters);
                    }

                    if (result.DataDraft != null && (context.ApiStatus == StatusForApi.All || context.IsFrontendClient))
                    {
                        result.DataDraft = result.DataDraft.ConvertName2Name(schema.SchemaDef, converters);
                    }
                    else
                    {
                        result.DataDraft = null;
                    }

                    yield return result;
                }
            }
        }

        private IEnumerable<FieldConverter> GenerateConverters(QueryContext context)
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

                if (context.Languages?.Any() == true)
                {
                    yield return FieldConverters.FilterLanguages(context.App.LanguagesConfig, context.Languages);
                }

                if (context.AssetUrlsToResolve?.Any() == true)
                {
                    yield return FieldConverters.ResolveAssetUrls(context.AssetUrlsToResolve,  assetUrlGenerator);
                }
            }
        }

        private Query ParseQuery(QueryContext context, string query, ISchemaEntity schema)
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

        public async Task<ISchemaEntity> GetSchemaOrThrowAsync(QueryContext context, string schemaIdOrName)
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

        private static void CheckPermission(ClaimsPrincipal user, params ISchemaEntity[] schemas)
        {
            var permissions = user.Permissions();

            foreach (var schema in schemas)
            {
                if (!HasPermission(permissions, schema))
                {
                    throw new DomainForbiddenException("You do not have permission for this schema.");
                }
            }
        }

        private static bool HasPermission(PermissionSet permissions, ISchemaEntity schema)
        {
            var permission = Permissions.ForApp(Permissions.AppContentsRead, schema.AppId.Name, schema.SchemaDef.Name);

            return permissions.Allows(permission);
        }

        private static Status[] GetStatus(QueryContext context)
        {
            if (context.IsFrontendClient || context.ApiStatus == StatusForApi.All)
            {
                return null;
            }
            else
            {
                return StatusPublishedOnly;
            }
        }

        private Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(QueryContext context, IReadOnlyList<Guid> ids, Status[] status)
        {
            return contentRepository.QueryAsync(context.App, status, new HashSet<Guid>(ids), ShouldIncludeDraft(context));
        }

        private Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, ISchemaEntity schema, Query query, Status[] status)
        {
            return contentRepository.QueryAsync(context.App, schema, status, context.IsFrontendClient, query, ShouldIncludeDraft(context));
        }

        private Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, ISchemaEntity schema, HashSet<Guid> ids, Status[] status)
        {
            return contentRepository.QueryAsync(context.App, schema, status, ids, ShouldIncludeDraft(context));
        }

        private Task<IContentEntity> FindContentAsync(QueryContext context, Guid id, Status[] status, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(context.App, schema, status, id, ShouldIncludeDraft(context));
        }

        private Task<IContentEntity> FindContentByVersionAsync(Guid id, long version)
        {
            return contentVersionLoader.LoadAsync(id, version);
        }

        private static bool ShouldIncludeDraft(QueryContext context)
        {
            return context.ApiStatus == StatusForApi.All || context.IsFrontendClient;
        }
    }
}
