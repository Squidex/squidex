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
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents.Edm;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;

// ReSharper disable InvertIf
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Squidex.Domain.Apps.Read.Contents
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private readonly IContentRepository contentRepository;
        private readonly ISchemaProvider schemas;
        private readonly IScriptEngine scriptEngine;
        private readonly EdmModelBuilder modelBuilder;

        public ContentQueryService(
            IContentRepository contentRepository,
            ISchemaProvider schemas,
            IScriptEngine scriptEngine,
            EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(schemas, nameof(schemas));

            this.contentRepository = contentRepository;
            this.schemas = schemas;
            this.scriptEngine = scriptEngine;
            this.modelBuilder = modelBuilder;
        }

        public async Task<(ISchemaEntity SchemaEntity, IContentEntity ContentEntity)> FindContentAsync(IAppEntity appEntity, string schemaIdOrName, ClaimsPrincipal user, Guid id)
        {
            Guard.NotNull(appEntity, nameof(appEntity));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schemaEntity = await FindSchemaAsync(appEntity, schemaIdOrName);

            var contentEntity = await contentRepository.FindContentAsync(appEntity, schemaEntity, id);

            if (contentEntity == null)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(ISchemaEntity));
            }

            contentEntity = TransformContent(user, schemaEntity, new List<IContentEntity> { contentEntity })[0];

            return (schemaEntity, contentEntity);
        }

        public async Task<(ISchemaEntity SchemaEntity, long Total, IReadOnlyList<IContentEntity> Items)> QueryWithCountAsync(IAppEntity appEntity, string schemaIdOrName, ClaimsPrincipal user, HashSet<Guid> ids, string query)
        {
            Guard.NotNull(appEntity, nameof(appEntity));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schemaEntity = await FindSchemaAsync(appEntity, schemaIdOrName);

            var parsedQuery = ParseQuery(appEntity, query, schemaEntity);

            var isFrontendClient = user.IsInClient("squidex-frontend");

            var taskForItems = contentRepository.QueryAsync(appEntity, schemaEntity, isFrontendClient, ids, parsedQuery);
            var taskForCount = contentRepository.CountAsync(appEntity, schemaEntity, isFrontendClient, ids, parsedQuery);

            await Task.WhenAll(taskForItems, taskForCount);

            var list = TransformContent(user, schemaEntity, taskForItems.Result.ToList());

            return (schemaEntity, taskForCount.Result, list);
        }

        private List<IContentEntity> TransformContent(ClaimsPrincipal user, ISchemaEntity schemaEntity, List<IContentEntity> contentEntities)
        {
            var scriptText = schemaEntity.ScriptQuery;

            if (!string.IsNullOrWhiteSpace(scriptText))
            {
                for (var i = 0; i < contentEntities.Count; i++)
                {
                    var contentEntity = contentEntities[i];
                    var contentData = scriptEngine.Transform(new ScriptContext { User = user, Data = contentEntity.Data, ContentId = contentEntity.Id }, scriptText);

                    contentEntities[i] = SimpleMapper.Map(contentEntity, new Content { Data = contentData });
                }
            }

            return contentEntities;
        }

        private ODataUriParser ParseQuery(IAppEntity appEntity, string query, ISchemaEntity schemaEntity)
        {
            try
            {
                var model = modelBuilder.BuildEdmModel(schemaEntity, appEntity);

                return model.ParseQuery(query);
            }
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }

        public async Task<ISchemaEntity> FindSchemaAsync(IEntity appEntity, string schemaIdOrName)
        {
            Guard.NotNull(appEntity, nameof(appEntity));

            ISchemaEntity schema = null;

            if (Guid.TryParse(schemaIdOrName, out var id))
            {
                schema = await schemas.FindSchemaByIdAsync(id);
            }

            if (schema == null)
            {
                schema = await schemas.FindSchemaByNameAsync(appEntity.Id, schemaIdOrName);
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaIdOrName, typeof(ISchemaEntity));
            }

            return schema;
        }

        private sealed class Content : IContentEntity
        {
            public Guid Id { get; set; }
            public Guid AppId { get; set; }
            public long Version { get; set; }
            public bool IsPublished { get; set; }
            public Instant Created { get; set; }
            public Instant LastModified { get; set; }
            public RefToken CreatedBy { get; set; }
            public RefToken LastModifiedBy { get; set; }
            public NamedContentData Data { get; set; }
        }
    }
}
