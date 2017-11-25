// ==========================================================================
//  RuleTriggerValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Rules.Guards
{
    public sealed class RuleTriggerValidator : IRuleTriggerVisitor<Task<IEnumerable<ValidationError>>>
    {
        public Func<Guid, Task<ISchemaEntity>> SchemaProvider { get; }

        public RuleTriggerValidator(Func<Guid, Task<ISchemaEntity>> schemaProvider)
        {
            SchemaProvider = schemaProvider;
        }

        public static Task<IEnumerable<ValidationError>> ValidateAsync(string appName, RuleTrigger action, IAppProvider appProvider)
        {
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(appProvider, nameof(appProvider));

            var visitor = new RuleTriggerValidator(x => appProvider.GetSchemaAsync(appName, x));

            return action.Accept(visitor);
        }

        public async Task<IEnumerable<ValidationError>> Visit(ContentChangedTrigger trigger)
        {
            if (trigger.Schemas != null)
            {
                var schemaErrors = await Task.WhenAll(
                    trigger.Schemas.Select(async s =>
                        await SchemaProvider(s.SchemaId) == null
                            ? new ValidationError($"Schema {s.SchemaId} does not exist.", nameof(trigger.Schemas))
                            : null));

                return schemaErrors.Where(x => x != null).ToList();
            }

            return new List<ValidationError>();
        }
    }
}
