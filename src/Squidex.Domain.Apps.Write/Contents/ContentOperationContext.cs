// ==========================================================================
//  ContentOperationContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.EnrichContent;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Domain.Apps.Write.Contents
{
    public sealed class ContentOperationContext
    {
        private ContentDomainObject content;
        private ContentCommand command;
        private IContentRepository contentRepository;
        private IAssetRepository assetRepository;
        private IScriptEngine scriptEngine;
        private ISchemaEntity schemaEntity;
        private IAppEntity appEntity;
        private Func<string> message;

        public static async Task<ContentOperationContext> CreateAsync(
            IContentRepository contentRepository,
            ContentDomainObject content,
            ContentCommand command,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            IScriptEngine scriptEngine,
            Func<string> message)
        {
            var (appEntity, schemaEntity) = await appProvider.GetAppWithSchemaAsync(command.AppId.Name, command.SchemaId.Id);

            var context = new ContentOperationContext();

            context.appEntity = appEntity;
            context.assetRepository = assetRepository;
            context.contentRepository = contentRepository;
            context.content = content;
            context.command = command;
            context.message = message;
            context.schemaEntity = schemaEntity;
            context.scriptEngine = scriptEngine;

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

        public async Task ValidateAsync(bool partial)
        {
            if (command is ContentDataCommand dataCommand)
            {
                var errors = new List<ValidationError>();

                var appId = command.AppId.Id;

                var ctx =
                    new ValidationContext(
                        (contentIds, schemaId) =>
                        {
                            return contentRepository.QueryNotFoundAsync(appId, schemaId, contentIds.ToList());
                        },
                        assetIds =>
                        {
                            return assetRepository.QueryNotFoundAsync(appId, assetIds.ToList());
                        });

                if (partial)
                {
                    await dataCommand.Data.ValidatePartialAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), errors);
                }
                else
                {
                    await dataCommand.Data.ValidateAsync(ctx, schemaEntity.SchemaDef, appEntity.PartitionResolver(), errors);
                }

                if (errors.Count > 0)
                {
                    throw new ValidationException(message(), errors.ToArray());
                }
            }
        }

        public Task ExecuteScriptAndTransformAsync(Func<ISchemaEntity, string> script, object operation)
        {
            if (command is ContentDataCommand dataCommand)
            {
                var ctx = new ScriptContext { ContentId = content.Id, OldData = content.Data, User = command.User, Operation = operation.ToString(), Data = dataCommand.Data };

                dataCommand.Data = scriptEngine.ExecuteAndTransform(ctx, script(schemaEntity));
            }

            return TaskHelper.Done;
        }

        public Task ExecuteScriptAsync(Func<ISchemaEntity, string> script, object operation)
        {
            var ctx = new ScriptContext { ContentId = content.Id, OldData = content.Data, User = command.User, Operation = operation.ToString() };

            scriptEngine.Execute(ctx, script(schemaEntity));

            return TaskHelper.Done;
        }
    }
}
