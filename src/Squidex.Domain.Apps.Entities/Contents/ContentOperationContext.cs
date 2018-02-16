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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.EnrichContent;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentOperationContext
    {
        private ContentCommand command;
        private IContentRepository contentRepository;
        private IContentEntity content;
        private IAssetRepository assetRepository;
        private IScriptEngine scriptEngine;
        private ISchemaEntity schemaEntity;
        private IAppEntity appEntity;
        private Guid appId;
        private Func<string> message;

        public static async Task<ContentOperationContext> CreateAsync(
            ContentCommand command,
            IContentEntity content,
            IContentRepository contentRepository,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            Func<string> message)
        {
            var a = content.AppId;
            var s = content.SchemaId;

            if (command is CreateContent createContent)
            {
                a = a ?? createContent.AppId;
                s = s ?? createContent.SchemaId;
            }

            var (appEntity, schemaEntity) = await appProvider.GetAppWithSchemaAsync(a.Id, s.Id);

            var context = new ContentOperationContext
            {
                appEntity = appEntity,
                appId = a.Id,
                assetRepository = assetRepository,
                contentRepository = contentRepository,
                content = content,
                command = command,
                message = message,
                schemaEntity = schemaEntity,
                scriptEngine = scriptEngine
            };

            return context;
        }

        public Task EnrichAsync()
        {
            if (command is ContentDataCommand dataCommand)
            {
                dataCommand.Data.Enrich(schemaEntity.SchemaDef, appEntity.PartitionResolver());
            }

            return TaskHelper.Done;
        }

        public Task ValidateAsync()
        {
            if (command is ContentDataCommand dataCommand)
            {
                var ctx = CreateValidationContext();

                return dataCommand.Data.ValidateAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), message);
            }

            return TaskHelper.Done;
        }

        public Task ValidatePartialAsync()
        {
            if (command is ContentDataCommand dataCommand)
            {
                var ctx = CreateValidationContext();

                return dataCommand.Data.ValidatePartialAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), message);
            }

            return TaskHelper.Done;
        }

        public Task ExecuteScriptAndTransformAsync(Func<ISchemaEntity, string> script, object operation)
        {
            if (command is ContentDataCommand dataCommand)
            {
                var ctx = CreateScriptContext(operation, dataCommand.Data);

                dataCommand.Data = scriptEngine.ExecuteAndTransform(ctx, script(schemaEntity));
            }

            return TaskHelper.Done;
        }

        public Task ExecuteScriptAsync(Func<ISchemaEntity, string> script, object operation)
        {
            var ctx = CreateScriptContext(operation, content.Data);

            scriptEngine.Execute(ctx, script(schemaEntity));

            return TaskHelper.Done;
        }

        private ScriptContext CreateScriptContext(object operation, NamedContentData data = null)
        {
            return new ScriptContext { ContentId = command.ContentId, OldData = content.Data, Data = data, User = command.User, Operation = operation.ToString() };
        }

        private ValidationContext CreateValidationContext()
        {
            return new ValidationContext(
                (contentIds, schemaId) =>
                {
                    return QueryContentsAsync(schemaId, contentIds);
                },
                assetIds =>
                {
                    return QueryAssetsAsync(assetIds);
                });
        }

        private async Task<IReadOnlyList<IAssetInfo>> QueryAssetsAsync(IEnumerable<Guid> assetIds)
        {
            return await assetRepository.QueryAsync(appId, new HashSet<Guid>(assetIds));
        }

        private async Task<IReadOnlyList<Guid>> QueryContentsAsync(Guid schemaId, IEnumerable<Guid> contentIds)
        {
            return await contentRepository.QueryNotFoundAsync(appId, schemaId, contentIds.ToList());
        }
    }
}
