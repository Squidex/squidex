// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.EnrichContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentOperationContext
    {
        private IContentRepository contentRepository;
        private IAssetRepository assetRepository;
        private IScriptEngine scriptEngine;
        private ISchemaEntity schemaEntity;
        private IAppEntity appEntity;
        private Guid contentId;
        private Guid schemaId;
        private Func<string> message;

        public ISchemaEntity Schema
        {
            get { return schemaEntity; }
        }

        public static async Task<ContentOperationContext> CreateAsync(
            Guid appId,
            Guid schemaId,
            Guid contentId,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IContentRepository contentRepository,
            IScriptEngine scriptEngine,
            Func<string> message)
        {
            var (appEntity, schemaEntity) = await appProvider.GetAppWithSchemaAsync(appId, schemaId);

            var context = new ContentOperationContext
            {
                appEntity = appEntity,
                assetRepository = assetRepository,
                contentId = contentId,
                contentRepository = contentRepository,
                message = message,
                schemaId = schemaId,
                schemaEntity = schemaEntity,
                scriptEngine = scriptEngine
            };

            return context;
        }

        public Task EnrichAsync(NamedContentData data)
        {
            data.Enrich(schemaEntity.SchemaDef, appEntity.PartitionResolver());

            return TaskHelper.Done;
        }

        public Task ValidateAsync(NamedContentData data)
        {
            var ctx = CreateValidationContext();

            return data.ValidateAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), message);
        }

        public Task ValidatePartialAsync(NamedContentData data)
        {
            var ctx = CreateValidationContext();

            return data.ValidatePartialAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), message);
        }

        public Task<NamedContentData> ExecuteScriptAndTransformAsync(Func<SchemaScripts, string> script, object operation, ContentCommand command, NamedContentData data, NamedContentData oldData = null)
        {
            var ctx = CreateScriptContext(operation, command, data, oldData);

            var result = scriptEngine.ExecuteAndTransform(ctx, GetScript(script));

            return Task.FromResult(result);
        }

        public Task ExecuteScriptAsync(Func<SchemaScripts, string> script, object operation, ContentCommand command, NamedContentData data, NamedContentData oldData = null)
        {
            var ctx = CreateScriptContext(operation, command, data, oldData);

            scriptEngine.Execute(ctx, GetScript(script));

            return TaskHelper.Done;
        }

        private static ScriptContext CreateScriptContext(object operation, ContentCommand command, NamedContentData data, NamedContentData oldData)
        {
            return new ScriptContext { ContentId = command.ContentId, OldData = oldData, Data = data, User = command.User, Operation = operation.ToString() };
        }

        private ValidationContext CreateValidationContext()
        {
            return new ValidationContext(contentId, schemaId, QueryContentsAsync, QueryAssetsAsync);
        }

        private async Task<IReadOnlyList<IAssetInfo>> QueryAssetsAsync(IEnumerable<Guid> assetIds)
        {
            return await assetRepository.QueryAsync(appEntity.Id, new HashSet<Guid>(assetIds));
        }

        private async Task<IReadOnlyList<Guid>> QueryContentsAsync(Guid filterSchemaId, FilterNode filterNode)
        {
            return await contentRepository.QueryIdsAsync(appEntity.Id, filterSchemaId, filterNode);
        }

        private string GetScript(Func<SchemaScripts, string> script)
        {
            return script(schemaEntity.SchemaDef.Scripts);
        }
    }
}
