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
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
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
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(contentVersionLoader, nameof(contentVersionLoader));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.contentRepository = contentRepository;
            this.contentVersionLoader = contentVersionLoader;
            this.appProvider = appProvider;
            this.scriptEngine = scriptEngine;
            this.modelBuilder = modelBuilder;
        }

        public Task ThrowIfSchemaNotExistsAsync(IAppEntity app, string schemaIdOrName)
        {
            return GetSchemaAsync(app, schemaIdOrName);
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, Guid id, long version = -1)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotEmpty(id, nameof(id));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schema = await GetSchemaAsync(app, schemaIdOrName);

            var isFrontendClient = IsFrontendClient(user);
            var isVersioned = version > EtagVersion.Empty;

            var content =
                isVersioned ?
                await FindContentByVersionAsync(id, version) :
                await FindContentAsync(app, id, schema);

            if (content == null || (content.Status != Status.Published && !isFrontendClient) || content.SchemaId.Id != schema.Id)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(ISchemaEntity));
            }

            content = TransformContent(app, schema, user, Enumerable.Repeat(content, 1), isVersioned, isFrontendClient).FirstOrDefault();

            return content;
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, string query)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schema = await GetSchemaAsync(app, schemaIdOrName);

            var isFrontendClient = IsFrontendClient(user);

            var parsedQuery = ParseQuery(app, query, schema);
            var parsedStatus = ParseStatus(isFrontendClient, archived);

            var contents = await contentRepository.QueryAsync(app, schema, parsedStatus.ToArray(), parsedQuery);

            return TransformContents(app, schema, user, contents, false, isFrontendClient);
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, HashSet<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schema = await GetSchemaAsync(app, schemaIdOrName);

            var isFrontendClient = IsFrontendClient(user);

            var parsedStatus = ParseStatus(isFrontendClient, archived);

            var contents = await contentRepository.QueryAsync(app, schema, parsedStatus.ToArray(), ids);

            return TransformContents(app, schema, user, contents, false, isFrontendClient);
        }

        private IResultList<IContentEntity> TransformContents(IAppEntity app, ISchemaEntity schema, ClaimsPrincipal user,
            IResultList<IContentEntity> contents,
            bool isTypeChecking,
            bool isFrontendClient)
        {
            var transformed = TransformContent(app, schema, user, contents, isTypeChecking, isFrontendClient);

            return ResultList.Create(transformed, contents.Total);
        }

        private IEnumerable<IContentEntity> TransformContent(IAppEntity app, ISchemaEntity schema, ClaimsPrincipal user,
            IEnumerable<IContentEntity> contents,
            bool isTypeChecking,
            bool isFrontendClient)
        {
            var scriptText = schema.ScriptQuery;

            var isScripting = !string.IsNullOrWhiteSpace(scriptText);

            foreach (var content in contents)
            {
                var result = SimpleMapper.Map(content, new ContentEntity());

                if (!isFrontendClient && isScripting)
                {
                    result.Data = scriptEngine.Transform(new ScriptContext { User = user, Data = content.Data, ContentId = content.Id }, scriptText);
                }

                result.Data = result.Data.ToApiModel(schema.SchemaDef, app.LanguagesConfig, isFrontendClient, isTypeChecking);

                yield return result;
            }
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

        public async Task<ISchemaEntity> GetSchemaAsync(IAppEntity app, string schemaIdOrName)
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

        private static List<Status> ParseStatus(bool isFrontendClient, bool archived)
        {
            var status = new List<Status>();

            if (isFrontendClient)
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

        private Task<IContentEntity> FindContentByVersionAsync(Guid id, long version)
        {
            return contentVersionLoader.LoadAsync(id, version);
        }

        private Task<IContentEntity> FindContentAsync(IAppEntity app, Guid id, ISchemaEntity schema)
        {
            return contentRepository.FindContentAsync(app, schema, id);
        }

        private static bool IsFrontendClient(ClaimsPrincipal user)
        {
            return user.IsInClient("squidex-frontend");
        }
    }
}
