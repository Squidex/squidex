// ==========================================================================
//  RuleTriggerValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Rules.Guards
{
    public sealed class RuleTriggerValidator : IRuleTriggerVisitor<Task<IEnumerable<ValidationError>>>
    {
        public ISchemaProvider Schemas { get; }

        public RuleTriggerValidator(ISchemaProvider schemas)
        {
            Schemas = schemas;
        }

        public static Task<IEnumerable<ValidationError>> ValidateAsync(RuleTrigger action, ISchemaProvider schemas)
        {
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(schemas, nameof(schemas));

            var visitor = new RuleTriggerValidator(schemas);

            return action.Accept(visitor);
        }

        public async Task<IEnumerable<ValidationError>> Visit(ContentChangedTrigger trigger)
        {
            if (trigger.Schemas != null)
            {
                var schemaErrors = await Task.WhenAll(
                    trigger.Schemas.Select(async s =>
                        await Schemas.FindSchemaByIdAsync(s.SchemaId) == null
                            ? new ValidationError($"Schema {s.SchemaId} does not exist.", nameof(trigger.Schemas))
                            : null));

                return schemaErrors.Where(x => x != null).ToList();
            }

            return new List<ValidationError>();
        }
    }
}
