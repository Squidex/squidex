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
using Squidex.Domain.Apps.Core.DefaultValues;
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
        private ContentCommand command;
        private Guid schemaId;
        private Func<string> message;

        public ISchemaEntity Schema
        {
            get { return schemaEntity; }
        }

        public static async Task<ContentOperationContext> CreateAsync(
            Guid appId,
            Guid schemaId,
            ContentCommand command,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IContentRepository contentRepository,
            IScriptEngine scriptEngine,
            Func<string> message)
        {
            var (appEntity, schemaEntity) = await appProvider.GetAppWithSchemaAsync(appId, schemaId);

            if (appEntity == null)
            {
                throw new InvalidOperationException("Cannot resolve app.");
            }

            if (schemaEntity == null)
            {
                throw new InvalidOperationException("Cannot resolve schema.");
            }

            var context = new ContentOperationContext
            {
                appEntity = appEntity,
                assetRepository = assetRepository,
                command = command,
                contentRepository = contentRepository,
                message = message,
                schemaId = schemaId,
                schemaEntity = schemaEntity,
                scriptEngine = scriptEngine
            };

            return context;
        }

        public Task GenerateDefaultValuesAsync(NamedContentData data)
        {
            data.GenerateDefaultValues(schemaEntity.SchemaDef, appEntity.PartitionResolver());

            return TaskHelper.Done;
        }

        public Task ValidateAsync(NamedContentData data, bool optimized)
        {
            var ctx = CreateValidationContext(optimized);

            return data.ValidateAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), message);
        }

        public Task ValidatePartialAsync(NamedContentData data, bool optimized)
        {
            var ctx = CreateValidationContext(optimized);

            return data.ValidatePartialAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), message);
        }

        public Task<NamedContentData> ExecuteScriptAndTransformAsync(Func<SchemaScripts, string> script, ScriptContext context)
        {
            Enrich(context);

            var result = scriptEngine.ExecuteAndTransform(context, GetScript(script));

            return Task.FromResult(result);
        }

        public Task ExecuteScriptAsync(Func<SchemaScripts, string> script, ScriptContext context)
        {
            Enrich(context);

            scriptEngine.Execute(context, GetScript(script));

            return TaskHelper.Done;
        }

        private void Enrich(ScriptContext context)
        {
            context.ContentId = command.ContentId;

            context.User = command.User;
        }

        private ValidationContext CreateValidationContext(bool optimized)
        {
            return new ValidationContext(command.ContentId, schemaId,
                    QueryContentsAsync,
                    QueryContentsAsync,
                    QueryAssetsAsync)
                .Optimized(optimized);
        }

        private async Task<IReadOnlyList<IAssetInfo>> QueryAssetsAsync(IEnumerable<Guid> assetIds)
        {
            return await assetRepository.QueryAsync(appEntity.Id, new HashSet<Guid>(assetIds));
        }

        private async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryContentsAsync(Guid filterSchemaId, FilterNode<ClrValue> filterNode)
        {
            return await contentRepository.QueryIdsAsync(appEntity.Id, filterSchemaId, filterNode);
        }

        private async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryContentsAsync(HashSet<Guid> ids)
        {
            return await contentRepository.QueryIdsAsync(appEntity.Id, ids, SearchScope.All);
        }

        private string GetScript(Func<SchemaScripts, string> script)
        {
            return script(schemaEntity.SchemaDef.Scripts);
        }
    }
}
