// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Extensions.Actions;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleActionProcessor : IDocumentProcessor
    {
        public async Task ProcessAsync(DocumentProcessorContext context)
        {
            var schema = context.SchemaResolver.GetSchema(typeof(RuleAction), false);

            if (schema != null)
            {
                var discriminator = new OpenApiDiscriminator
                {
                    JsonInheritanceConverter = new JsonInheritanceConverter("actionType", typeof(RuleAction)),
                    PropertyName = "actionType"
                };

                schema.DiscriminatorObject = discriminator;
                schema.Properties["actionType"] = new JsonProperty
                {
                    Type = JsonObjectType.String,
                    IsRequired = true
                };

                foreach (var derived in RuleElementRegistry.Actions)
                {
                    var derivedSchema = await context.SchemaGenerator.GenerateAsync(derived.Value.Type, context.SchemaResolver);

                    var oldName = context.Document.Definitions.FirstOrDefault(x => x.Value == derivedSchema).Key;

                    if (oldName != null)
                    {
                        context.Document.Definitions.Remove(oldName);
                        context.Document.Definitions.Add(derived.Key, derivedSchema);
                    }
                }

                RemoveFreezable(context, schema);
            }
        }

        private static void RemoveFreezable(DocumentProcessorContext context, JsonSchema4 schema)
        {
            context.Document.Definitions.Remove("Freezable");

            schema.AllOf.Clear();
        }
    }
}
