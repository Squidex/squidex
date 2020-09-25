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
using Squidex.Domain.Apps.Core.DefaultValues;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Validation;

#pragma warning disable IDE0016 // Use 'throw' expression

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentOperationContext
    {
        private static readonly ScriptOptions ScriptOptions = new ScriptOptions
        {
            AsContext = true,
            CanDisallow = true,
            CanReject = true
        };

        private readonly IScriptEngine scriptEngine;
        private readonly ISemanticLog log;
        private readonly IAppProvider appProvider;
        private readonly IEnumerable<IValidatorsFactory> factories;
        private ISchemaEntity schema;
        private IAppEntity app;
        private ContentCommand command;
        private ValidationContext validationContext;

        public ContentOperationContext(IAppProvider appProvider, IEnumerable<IValidatorsFactory> factories, IScriptEngine scriptEngine, ISemanticLog log)
        {
            this.appProvider = appProvider;
            this.factories = factories;
            this.scriptEngine = scriptEngine;

            this.log = log;
        }

        public ISchemaEntity Schema
        {
            get { return schema; }
        }

        public async Task LoadAsync(NamedId<DomainId> appId, NamedId<DomainId> schemaId, ContentCommand command, bool optimized)
        {
            this.command = command;

            var (app, schema) = await appProvider.GetAppWithSchemaAsync(appId.Id, schemaId.Id);

            if (app == null)
            {
                throw new DomainObjectNotFoundException(appId.ToString());
            }

            this.app = app;

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaId.ToString());
            }

            this.schema = schema;

            validationContext = new ValidationContext(appId, schemaId, schema.SchemaDef, command.ContentId).Optimized(optimized);
        }

        public Task GenerateDefaultValuesAsync(NamedContentData data)
        {
            data.GenerateDefaultValues(schema.SchemaDef, app.PartitionResolver());

            return Task.CompletedTask;
        }

        public async Task ValidateInputAsync(NamedContentData data)
        {
            var validator = new ContentValidator(app.PartitionResolver(), validationContext, factories, log);

            await validator.ValidateInputAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateInputPartialAsync(NamedContentData data)
        {
            var validator = new ContentValidator(app.PartitionResolver(), validationContext, factories, log);

            await validator.ValidateInputPartialAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateContentAsync(NamedContentData data)
        {
            var validator = new ContentValidator(app.PartitionResolver(), validationContext, factories, log);

            await validator.ValidateContentAsync(data);

            CheckErrors(validator);
        }

        private static void CheckErrors(ContentValidator validator)
        {
            if (validator.Errors.Count > 0)
            {
                throw new ValidationException(validator.Errors.ToList());
            }
        }

        public async Task<NamedContentData> ExecuteScriptAndTransformAsync(Func<SchemaScripts, string> script, ScriptVars context)
        {
            Enrich(context);

            var actualScript = GetScript(script);

            if (string.IsNullOrWhiteSpace(actualScript))
            {
                return context.Data!;
            }

            return await scriptEngine.TransformAsync(context, actualScript, ScriptOptions);
        }

        public async Task ExecuteScriptAsync(Func<SchemaScripts, string> script, ScriptVars context)
        {
            Enrich(context);

            var actualScript = GetScript(script);

            if (string.IsNullOrWhiteSpace(actualScript))
            {
                return;
            }

            await scriptEngine.ExecuteAsync(context, GetScript(script), ScriptOptions);
        }

        private void Enrich(ScriptVars context)
        {
            context.ContentId = command.ContentId;
            context.AppId = app.Id;
            context.AppName = app.Name;
            context.User = command.User;
        }

        private string GetScript(Func<SchemaScripts, string> script)
        {
            return script(schema.SchemaDef.Scripts);
        }
    }
}
