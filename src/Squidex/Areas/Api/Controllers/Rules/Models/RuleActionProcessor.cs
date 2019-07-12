// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleActionProcessor : IDocumentProcessor
    {
        private readonly RuleRegistry ruleRegistry;

        public RuleActionProcessor(RuleRegistry ruleRegistry)
        {
            Guard.NotNull(ruleRegistry, nameof(ruleRegistry));

            this.ruleRegistry = ruleRegistry;
        }

        public async Task ProcessAsync(DocumentProcessorContext context)
        {
            try
            {
                var schema = context.SchemaResolver.GetSchema(typeof(RuleAction), false);

                if (schema != null)
                {
                    schema.DiscriminatorObject = new OpenApiDiscriminator
                    {
                        JsonInheritanceConverter = new RuleActionConverter(), PropertyName = "actionType"
                    };

                    schema.Properties["actionType"] = new JsonProperty
                    {
                        Type = JsonObjectType.String, IsRequired = true
                    };

                    foreach (var action in ruleRegistry.Actions)
                    {
                        var derivedSchema = await context.SchemaGenerator.GenerateAsync(action.Value.Type, context.SchemaResolver);

                        var oldName = context.Document.Definitions.FirstOrDefault(x => x.Value == derivedSchema).Key;

                        if (oldName != null)
                        {
                            context.Document.Definitions.Remove(oldName);
                            context.Document.Definitions.Add($"{action.Key}RuleActionDto", derivedSchema);
                        }
                    }

                    RemoveFreezable(context, schema);
                }
            }
            catch (KeyNotFoundException)
            {
                return;
            }
        }

        private static void RemoveFreezable(DocumentProcessorContext context, JsonSchema4 schema)
        {
            context.Document.Definitions.Remove("Freezable");

            schema.AllOf.Clear();
        }
    }
}
