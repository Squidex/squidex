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
using Squidex.Shared;
using Squidex.Shared.Identity;

#pragma warning disable RECS0147

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private static readonly Status[] StatusAll = { Status.Archived, Status.Draft, Status.Published };
        private static readonly Status[] StatusArchived = { Status.Archived };
        private static readonly Status[] StatusPublished = { Status.Published };
        private static readonly Status[] StatusDraftOrPublished = { Status.Draft, Status.Published };
        private readonly IContentRepository contentRepository;
        private readonly IContentVersionLoader contentVersionLoader;
        private readonly IAppProvider appProvider;
        private readonly IAssetUrlGenerator assetUrlGenerator;
        private readonly IScriptEngine scriptEngine;
        private readonly ContentOptions options;
        private readonly EdmModelBuilder modelBuilder;

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

        public Task ThrowIfSchemaNotExistsAsync(QueryContext context, string schemaIdOrName)
        {
            return GetSchemaAsync(context, schemaIdOrName);
        }

        public async Task<IContentEntity> FindContentAsync(QueryContext context, string schemaIdOrName, Guid id, long version = -1)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaAsync(context, schemaIdOrName);

            CheckPermission(schema, context.User);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var isVersioned = version > EtagVersion.Empty;

                var status = GetFindStatus(context);

                var content =
                    isVersioned ?
                    await FindContentByVersionAsync(id, version) :
                    await FindContentAsync(context, id, status, schema);

                if (content == null || (content.Status != Status.Published && !context.IsFrontendClient) || content.SchemaId.Id != schema.Id)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
                }

                return Transform(context, schema, true, content);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, string schemaIdOrName, Q query)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaAsync(context, schemaIdOrName);

            CheckPermission(schema, context.User);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var status = GetQueryStatus(context);

                IResultList<IContentEntity> contents;

                if (query.Ids?.Count > 0)
                {
                    contents = await contentRepository.QueryAsync(context.App, schema, status, new HashSet<Guid>(query.Ids));
                    contents = Sort(contents, query.Ids);
                }
                else
                {
                    var parsedQuery = ParseQuery(context, query.ODataQuery, schema);

                    contents = await contentRepository.QueryAsync(context.App, schema, status, parsedQuery);
                }

                return Transform(context, schema, true, contents);
            }
        }

        private IContentEntity Transform(QueryContext context, ISchemaEntity schema, bool checkType, IContentEntity content)
        {
            return TransformCore(context, schema, checkType, Enumerable.Repeat(content, 1)).FirstOrDefault();
        }

        private IResultList<IContentEntity> Transform(QueryContext context, ISchemaEntity schema, bool checkType, IResultList<IContentEntity> contents)
        {
            var transformed = TransformCore(context, schema, checkType, contents);

            return ResultList.Create(contents.Total, transformed);
        }

        private static IResultList<IContentEntity> Sort(IResultList<IContentEntity> contents, IReadOnlyList<Guid> ids)
        {
            var sorted = ids.Select(id => contents.FirstOrDefault(x => x.Id == id)).Where(x => x != null);

            return ResultList.Create(contents.Total, sorted);
        }

        private IEnumerable<IContentEntity> TransformCore(QueryContext context, ISchemaEntity schema, bool checkType, IEnumerable<IContentEntity> contents)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var converters = GenerateConverters(context, checkType).ToArray();

                var scriptText = schema.SchemaDef.Scripts.Query;

                var isScripting = !string.IsNullOrWhiteSpace(scriptText);

                foreach (var content in contents)
                {
                    var result = SimpleMapper.Map(content, new ContentEntity());

                    if (result.Data != null)
                    {
                        if (!context.IsFrontendClient && isScripting)
                        {
                            result.Data = scriptEngine.Transform(new ScriptContext { User = context.User, Data = content.Data, ContentId = content.Id }, scriptText);
                        }

                        result.Data = result.Data.ConvertName2Name(schema.SchemaDef, converters);
                    }

                    if (result.DataDraft != null)
                    {
                        result.DataDraft = result.DataDraft.ConvertName2Name(schema.SchemaDef, converters);
                    }

                    yield return result;
                }
            }
        }

        private IEnumerable<FieldConverter> GenerateConverters(QueryContext context, bool checkType)
        {
            if (!context.IsFrontendClient)
            {
                yield return FieldConverters.ExcludeHidden();
                yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeHidden());
            }

            if (checkType)
            {
                yield return FieldConverters.ExcludeChangedTypes();
                yield return FieldConverters.ForNestedName2Name(ValueConverters.ExcludeChangedTypes());
            }

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
                    var model = modelBuilder.BuildEdmModel(schema, context.App);

                    var result = model.ParseQuery(query).ToQuery();

                    if (result.Sort.Count == 0)
                    {
                        result.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
                    }

                    if (result.Take > options.MaxResults)
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

        public async Task<ISchemaEntity> GetSchemaAsync(QueryContext context, string schemaIdOrName)
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

        private static void CheckPermission(ISchemaEntity schema, ClaimsPrincipal user)
        {
            var permissions = user.Permissions();
            var permission = Permissions.ForApp(Permissions.AppContentsRead, schema.AppId.Name, schema.SchemaDef.Name);

            if (!permissions.Allows(permission))
            {
                throw new DomainForbiddenException("You do not have permission for this schema.");
            }
        }

        private static Status[] GetFindStatus(QueryContext context)
        {
            if (context.IsFrontendClient)
            {
                return StatusAll;
            }
            else if (context.Unpublished)
            {
                return StatusDraftOrPublished;
            }
            else
            {
                return StatusPublished;
            }
        }

        private static Status[] GetQueryStatus(QueryContext context)
        {
            if (context.IsFrontendClient)
            {
                if (context.Archived)
                {
                    return StatusArchived;
                }
                else
                {
                    return StatusDraftOrPublished;
                }
            }
            else
            {
                if (context.Unpublished)
                {
                    return StatusDraftOrPublished;
                }
                else
                {
                    return StatusPublished;
                }
            }
        }

        private Task<IContentEntity> FindContentByVersionAsync(Guid id, long version)
        {
            return contentVersionLoader.LoadAsync(id, version);
        }

        private Task<IContentEntity> FindContentAsync(QueryContext context, Guid id, Status[] status, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(context.App, schema, status, id);
        }
    }
}
