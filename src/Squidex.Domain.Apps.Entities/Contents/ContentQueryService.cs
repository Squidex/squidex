// ==========================================================================
//  ContentQueryService.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private readonly IContentRepository contentRepository;
        private readonly IAppProvider appProvider;
        private readonly IScriptEngine scriptEngine;
        private readonly EdmModelBuilder modelBuilder;

        public ContentQueryService(
            IContentRepository contentRepository,
            IAppProvider appProvider,
            IScriptEngine scriptEngine,
            EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.contentRepository = contentRepository;
            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.modelBuilder = modelBuilder;
        }

        public async Task<(ISchemaEntity Schema, IContentEntity Content)> FindContentAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, Guid id, long version = -1)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var isFrontendClient = user.IsInClient("squidex-frontend");

            var schema = await FindSchemaAsync(app, schemaIdOrName);

            var content =
                version > EtagVersion.Empty ?
                await contentRepository.FindContentAsync(app, schema, id, version) :
                await contentRepository.FindContentAsync(app, schema, id);

            if (content == null || (content.Status != Status.Published && !isFrontendClient))
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(ISchemaEntity));
            }

            content = TransformContent(user, schema, new List<IContentEntity> { content })[0];

            return (schema, content);
        }

        public async Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> QueryWithCountAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, string query)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schema = await FindSchemaAsync(app, schemaIdOrName);

            var parsedQuery = ParseQuery(app, query, schema);

            var status = ParseStatus(user, archived);

            var taskForItems = contentRepository.QueryAsync(app, schema, status.ToArray(), parsedQuery);
            var taskForCount = contentRepository.CountAsync(app, schema, status.ToArray(), parsedQuery);

            await Task.WhenAll(taskForItems, taskForCount);

            var list = TransformContent(user, schema, taskForItems.Result.ToList());

            return (schema, taskForCount.Result, list);
        }

        public async Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> QueryWithCountAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, HashSet<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schema = await FindSchemaAsync(app, schemaIdOrName);

            var status = ParseStatus(user, archived);

            var taskForItems = contentRepository.QueryAsync(app, schema, status.ToArray(), ids);
            var taskForCount = contentRepository.CountAsync(app, schema, status.ToArray(), ids);

            await Task.WhenAll(taskForItems, taskForCount);

            var list = TransformContent(user, schema, taskForItems.Result.ToList());

            return (schema, taskForCount.Result, list);
        }

        private List<IContentEntity> TransformContent(ClaimsPrincipal user, ISchemaEntity schema, List<IContentEntity> contents)
        {
            var scriptText = schema.ScriptQuery;

            if (!string.IsNullOrWhiteSpace(scriptText))
            {
                for (var i = 0; i < contents.Count; i++)
                {
                    var content = contents[i];
                    var contentData = scriptEngine.Transform(new ScriptContext { User = user, Data = content.Data, ContentId = content.Id }, scriptText);

                    contents[i] = SimpleMapper.Map(content, new Content { Data = contentData });
                }
            }

            return contents;
        }

        private ODataUriParser ParseQuery(IAppEntity app, string query, ISchemaEntity schema)
        {
            try
            {
                var model = modelBuilder.BuildEdmModel(schema, app);

                return model.ParseQuery(query);
            }
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }

        public async Task<ISchemaEntity> FindSchemaAsync(IAppEntity app, string schemaIdOrName)
        {
            Guard.NotNull(app, nameof(app));

            ISchemaEntity schema = null;

            if (Guid.TryParse(schemaIdOrName, out var id))
            {
                schema = await appProvider.GetSchemaAsync(app.Id, id);
            }

            if (schema == null)
            {
                schema = await appProvider.GetSchemaAsync(app.Id, schemaIdOrName);
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaIdOrName, typeof(ISchemaEntity));
            }

            return schema;
        }

        private static List<Status> ParseStatus(ClaimsPrincipal user, bool archived)
        {
            var status = new List<Status>();

            if (user.IsInClient("squidex-frontend"))
            {
                if (archived)
                {
                    status.Add(Status.Archived);
                }
                else
                {
                    status.Add(Status.Draft);
                    status.Add(Status.Published);
                }
            }
            else
            {
                status.Add(Status.Published);
            }

            return status;
        }

        private sealed class Content : IContentEntity
        {
            public Guid Id { get; set; }
            public Guid AppId { get; set; }

            public long Version { get; set; }

            public Instant Created { get; set; }
            public Instant LastModified { get; set; }

            public RefToken CreatedBy { get; set; }
            public RefToken LastModifiedBy { get; set; }

            public NamedContentData Data { get; set; }

            public Status Status { get; set; }
        }
    }
}
