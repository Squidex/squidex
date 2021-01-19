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
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.DefaultValues;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Validation;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
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
        private readonly IEnumerable<IValidatorsFactory> validators;
        private readonly IContentWorkflow contentWorkflow;
        private readonly IContentRepository contentRepository;
        private readonly IJsonSerializer jsonSerializer;
        private ISchemaEntity schema;
        private IAppEntity app;
        private ContentCommand command;
        private ValidationContext validationContext;

        public IContentWorkflow Workflow => contentWorkflow;

        public IContentRepository Repository => contentRepository;

        public ContentOperationContext(
            IAppProvider appProvider,
            IEnumerable<IValidatorsFactory> validators,
            IContentWorkflow contentWorkflow,
            IContentRepository contentRepository,
            IJsonSerializer jsonSerializer,
            IScriptEngine scriptEngine,
            ISemanticLog log)
        {
            Guard.NotDefault(appProvider, nameof(appProvider));
            Guard.NotDefault(validators, nameof(validators));
            Guard.NotDefault(contentWorkflow, nameof(contentWorkflow));
            Guard.NotDefault(contentRepository, nameof(contentRepository));
            Guard.NotDefault(jsonSerializer, nameof(jsonSerializer));
            Guard.NotDefault(scriptEngine, nameof(scriptEngine));
            Guard.NotDefault(log, nameof(log));

            this.appProvider = appProvider;
            this.validators = validators;
            this.contentWorkflow = contentWorkflow;
            this.contentRepository = contentRepository;
            this.jsonSerializer = jsonSerializer;
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

            validationContext = new ValidationContext(jsonSerializer, appId, schemaId, schema.SchemaDef, command.ContentId).Optimized(optimized);
        }

        public Task<Status> GetInitialStatusAsync()
        {
            return contentWorkflow.GetInitialStatusAsync(schema);
        }

        public Task GenerateDefaultValuesAsync(ContentData data)
        {
            data.GenerateDefaultValues(schema.SchemaDef, Partition());

            return Task.CompletedTask;
        }

        public async Task ValidateInputAsync(ContentData data, bool publish)
        {
            var validator =
                new ContentValidator(Partition(),
                    validationContext.AsPublishing(publish), validators, log);

            await validator.ValidateInputAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateInputPartialAsync(ContentData data)
        {
            var validator =
                new ContentValidator(Partition(),
                    validationContext, validators, log);

            await validator.ValidateInputPartialAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateContentAsync(ContentData data)
        {
            var validator =
                new ContentValidator(Partition(),
                    validationContext, validators, log);

            await validator.ValidateContentAsync(data);

            CheckErrors(validator);
        }

        public async Task ValidateContentAndInputAsync(ContentData data)
        {
            var validator =
                new ContentValidator(Partition(),
                    validationContext.AsPublishing(), validators, log);

            await validator.ValidateInputAsync(data);
            await validator.ValidateContentAsync(data);

            CheckErrors(validator);
        }

        public Task ValidateOnPublishAsync(ContentData data)
        {
            if (!schema.SchemaDef.Properties.ValidateOnPublish)
            {
                return Task.CompletedTask;
            }

            return ValidateContentAndInputAsync(data);
        }

        private static void CheckErrors(ContentValidator validator)
        {
            if (validator.Errors.Count > 0)
            {
                throw new ValidationException(validator.Errors.ToList());
            }
        }

        public bool HasScript(Func<SchemaScripts, string> script)
        {
            return !string.IsNullOrWhiteSpace(GetScript(script));
        }

        public async Task<ContentData> ExecuteScriptAndTransformAsync(Func<SchemaScripts, string> script, ScriptVars context)
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

        private PartitionResolver Partition()
        {
            return app.PartitionResolver();
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
