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
        private IContentEntity contentEntity;
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
            IContentEntity contentEntity,
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
                contentEntity = contentEntity,
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

        public Task<NamedContentData> ExecuteScriptAndTransformAsync(Func<SchemaScripts, string> script, object operation, ContentCommand command,
            NamedContentData data, Status? status = null)
        {
            var ctx = CreateScriptContext(operation, command, data, status);

            var result = scriptEngine.ExecuteAndTransform(ctx, GetScript(script));

            return Task.FromResult(result);
        }

        public Task ExecuteScriptAsync(Func<SchemaScripts, string> script, object operation, ContentCommand command,
            NamedContentData data, Status? status = null)
        {
            var ctx = CreateScriptContext(operation, command, data, status);

            scriptEngine.Execute(ctx, GetScript(script));

            return TaskHelper.Done;
        }

        private ScriptContext CreateScriptContext(object operation, ContentCommand command, NamedContentData data, Status? status)
        {
            var result = new ScriptContext { ContentId = command.ContentId, Data = data, User = command.User, Operation = operation.ToString() };

            if (data != null)
            {
                result.Data = data;
                result.DataOld = contentEntity?.Data;
            }
            else
            {
                result.Data = contentEntity?.Data;
            }

            if (status.HasValue)
            {
                result.Status = status.Value;
                result.StatusOld = contentEntity?.Status ?? default;
            }
            else
            {
                result.Status = contentEntity?.Status ?? default;
            }

            return result;
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
