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
using Squidex.Infrastructure.Validation;

#pragma warning disable IDE0016 // Use 'throw' expression

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentOperationContext
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IAppProvider appProvider;
        private readonly IEnumerable<IValidatorsFactory> factories;
        private ISchemaEntity schema;
        private IAppEntity app;
        private ContentCommand command;
        private ValidationContext validationContext;
        private Func<string> message;

        public ContentOperationContext(IAppProvider appProvider, IEnumerable<IValidatorsFactory> factories, IScriptEngine scriptEngine)
        {
            this.appProvider = appProvider;
            this.factories = factories;
            this.scriptEngine = scriptEngine;
        }

        public ISchemaEntity Schema
        {
            get { return schema; }
        }

        public async Task LoadAsync(NamedId<Guid> appId, NamedId<Guid> schemaId, ContentCommand command, Func<string> message, bool optimized)
        {
            var (app, schema) = await appProvider.GetAppWithSchemaAsync(appId.Id, schemaId.Id);

            if (app == null)
            {
                throw new InvalidOperationException("Cannot resolve app.");
            }

            if (schema == null)
            {
                throw new InvalidOperationException("Cannot resolve schema.");
            }

            this.app = app;
            this.schema = schema;
            this.command = command;
            this.message = message;

            validationContext = new ValidationContext(appId, schemaId, schema.SchemaDef, command.ContentId).Optimized(optimized);
        }

        public Task GenerateDefaultValuesAsync(NamedContentData data)
        {
            data.GenerateDefaultValues(schema.SchemaDef, app.PartitionResolver());

            return Task.CompletedTask;
        }

        public async Task ValidateInputAsync(NamedContentData data)
        {
            var validator = new ContentValidator(app.PartitionResolver(), validationContext, factories);

            await validator.ValidateInputAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateInputPartialAsync(NamedContentData data)
        {
            var validator = new ContentValidator(app.PartitionResolver(), validationContext, factories);

            await validator.ValidateInputPartialAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateContentAsync(NamedContentData data)
        {
            var validator = new ContentValidator(app.PartitionResolver(), validationContext, factories);

            await validator.ValidateContentAsync(data);

            CheckErrors(validator);
        }

        private void CheckErrors(ContentValidator validator)
        {
            if (validator.Errors.Count > 0)
            {
                throw new ValidationException(message(), validator.Errors.ToList());
            }
        }

        public async Task<NamedContentData> ExecuteScriptAndTransformAsync(Func<SchemaScripts, string> script, ScriptContext context)
        {
            Enrich(context);

            var actualScript = GetScript(script);

            if (string.IsNullOrWhiteSpace(actualScript))
            {
                return context.Data!;
            }

            return await scriptEngine.ExecuteAndTransformAsync(context, actualScript);
        }

        public async Task ExecuteScriptAsync(Func<SchemaScripts, string> script, ScriptContext context)
        {
            Enrich(context);

            var actualScript = GetScript(script);

            if (string.IsNullOrWhiteSpace(actualScript))
            {
                return;
            }

            await scriptEngine.ExecuteAsync(context, GetScript(script));
        }

        private void Enrich(ScriptContext context)
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
