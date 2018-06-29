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
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

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
        private readonly IScriptEngine scriptEngine;
        private readonly EdmModelBuilder modelBuilder;

        public ContentQueryService(
            IContentRepository contentRepository,
            IContentVersionLoader contentVersionLoader,
            IAppProvider appProvider,
            IScriptEngine scriptEngine,
            EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(contentVersionLoader, nameof(contentVersionLoader));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.appProvider = appProvider;
            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.modelBuilder = modelBuilder;
            this.scriptEngine = scriptEngine;
        }

        public Task ThrowIfSchemaNotExistsAsync(QueryContext context)
        {
            return GetSchemaAsync(context);
        }

        public async Task<IContentEntity> FindContentAsync(QueryContext context, Guid id, long version = -1)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaAsync(context);

            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var isVersioned = version > EtagVersion.Empty;

                var parsedStatus = context.IsFrontendClient ? StatusAll : StatusPublished;

                var content =
                    isVersioned ?
                    await FindContentByVersionAsync(id, version) :
                    await FindContentAsync(context, id, parsedStatus, schema);

                if (content == null || (content.Status != Status.Published && !context.IsFrontendClient) || content.SchemaId.Id != schema.Id)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(ISchemaEntity));
                }

                return Transform(context, schema, true, content);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, string query)
        {
            Guard.NotNull(context, nameof(context));

            var schema = await GetSchemaAsync(context);

            using (Profiler.TraceMethod<ContentQueryService>("QueryAsyncByQuery"))
            {
                var parsedQuery = ParseQuery(context, query, schema);
                var parsedStatus = ParseStatus(context);

                var contents = await contentRepository.QueryAsync(context.App, schema, parsedStatus, parsedQuery);

                return Transform(context, schema, true, contents);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(QueryContext context, IList<Guid> ids)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(ids, nameof(ids));

            var schema = await GetSchemaAsync(context);

            using (Profiler.TraceMethod<ContentQueryService>("QueryAsyncByIds"))
            {
                var parsedStatus = ParseStatus(context);

                var contents = await contentRepository.QueryAsync(context.App, schema, parsedStatus, new HashSet<Guid>(ids));

                return Sort(Transform(context, schema, false, contents), ids);
            }
        }

        private IContentEntity Transform(QueryContext context, ISchemaEntity schema, bool checkType, IContentEntity content)
        {
            return Transform(context, schema, checkType, Enumerable.Repeat(content, 1)).FirstOrDefault();
        }

        private IResultList<IContentEntity> Transform(QueryContext context, ISchemaEntity schema, bool checkType, IResultList<IContentEntity> contents)
        {
            var transformed = Transform(context, schema, checkType, (IEnumerable<IContentEntity>)contents);

            return ResultList.Create(transformed, contents.Total);
        }

        private IResultList<IContentEntity> Sort(IResultList<IContentEntity> contents, IList<Guid> ids)
        {
            var sorted = ids.Select(id => contents.FirstOrDefault(x => x.Id == id)).Where(x => x != null);

            return ResultList.Create(sorted, contents.Total);
        }

        private IEnumerable<IContentEntity> Transform(QueryContext context, ISchemaEntity schema, bool checkType, IEnumerable<IContentEntity> contents)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                var converters = GenerateConverters(context, checkType).ToArray();

                var scriptText = schema.ScriptQuery;

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
            }
        }

        private ODataUriParser ParseQuery(QueryContext context, string query, ISchemaEntity schema)
        {
            using (Profiler.TraceMethod<ContentQueryService>())
            {
                try
                {
                    var model = modelBuilder.BuildEdmModel(schema, context.App);

                    return model.ParseQuery(query);
                }
                catch (ODataException ex)
                {
                    throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
                }
            }
        }

        public async Task<ISchemaEntity> GetSchemaAsync(QueryContext context)
        {
            ISchemaEntity schema = null;

            if (Guid.TryParse(context.SchemaIdOrName, out var id))
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, id);
            }

            if (schema == null)
            {
                schema = await appProvider.GetSchemaAsync(context.App.Id, context.SchemaIdOrName);
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(context.SchemaIdOrName, typeof(ISchemaEntity));
            }

            return schema;
        }

        private static Status[] ParseStatus(QueryContext context)
        {
            if (context.IsFrontendClient)
            {
                if (context.Archived)
                {
                    return StatusArchived;
                }

                return StatusDraftOrPublished;
            }

            return StatusPublished;
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
