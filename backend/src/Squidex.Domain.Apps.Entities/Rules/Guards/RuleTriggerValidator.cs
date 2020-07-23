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
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public sealed class RuleTriggerValidator : IRuleTriggerVisitor<Task<IEnumerable<ValidationError>>>
    {
        public Func<Guid, Task<ISchemaEntity?>> SchemaProvider { get; }

        public RuleTriggerValidator(Func<Guid, Task<ISchemaEntity?>> schemaProvider)
        {
            SchemaProvider = schemaProvider;
        }

        public static Task<IEnumerable<ValidationError>> ValidateAsync(Guid appId, RuleTrigger action, IAppProvider appProvider)
        {
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(appProvider, nameof(appProvider));

            var visitor = new RuleTriggerValidator(x => appProvider.GetSchemaAsync(appId, x, false));

            return action.Accept(visitor);
        }

        public Task<IEnumerable<ValidationError>> Visit(CommentTrigger trigger)
        {
            return Task.FromResult(Enumerable.Empty<ValidationError>());
        }

        public Task<IEnumerable<ValidationError>> Visit(AssetChangedTriggerV2 trigger)
        {
            return Task.FromResult(Enumerable.Empty<ValidationError>());
        }

        public Task<IEnumerable<ValidationError>> Visit(ManualTrigger trigger)
        {
            return Task.FromResult(Enumerable.Empty<ValidationError>());
        }

        public Task<IEnumerable<ValidationError>> Visit(SchemaChangedTrigger trigger)
        {
            return Task.FromResult(Enumerable.Empty<ValidationError>());
        }

        public Task<IEnumerable<ValidationError>> Visit(UsageTrigger trigger)
        {
            var errors = new List<ValidationError>();

            if (trigger.NumDays.HasValue && (trigger.NumDays < 1 || trigger.NumDays > 30))
            {
                errors.Add(new ValidationError(Not.Between(nameof(trigger.NumDays), 1, 30), nameof(trigger.NumDays)));
            }

            return Task.FromResult<IEnumerable<ValidationError>>(errors);
        }

        public async Task<IEnumerable<ValidationError>> Visit(ContentChangedTriggerV2 trigger)
        {
            var errors = new List<ValidationError>();

            if (trigger.Schemas != null)
            {
                var tasks = new List<Task<ValidationError?>>();

                foreach (var schema in trigger.Schemas)
                {
                    if (schema.SchemaId == Guid.Empty)
                    {
                        errors.Add(new ValidationError(Not.Defined("SchemaId"), nameof(trigger.Schemas)));
                    }
                    else
                    {
                        tasks.Add(CheckSchemaAsync(schema));
                    }
                }

                var checkErrors = await Task.WhenAll(tasks);

                errors.AddRange(checkErrors.NotNull());
            }

            return errors;
        }

        private async Task<ValidationError?> CheckSchemaAsync(ContentChangedTriggerSchemaV2 schema)
        {
            if (await SchemaProvider(schema.SchemaId) == null)
            {
                return new ValidationError(T.Get("schemas.notFoundId", new { id = schema.SchemaId }), nameof(ContentChangedTriggerV2.Schemas));
            }

            return null;
        }
    }
}
