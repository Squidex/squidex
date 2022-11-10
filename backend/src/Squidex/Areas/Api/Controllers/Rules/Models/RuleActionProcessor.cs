// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Namotion.Reflection;
using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class RuleActionProcessor : IDocumentProcessor
{
    private readonly RuleTypeProvider ruleRegistry;

    public RuleActionProcessor(RuleTypeProvider ruleRegistry)
    {
        this.ruleRegistry = ruleRegistry;
    }

    public void Process(DocumentProcessorContext context)
    {
        try
        {
            var schema = context.SchemaResolver.GetSchema(typeof(RuleAction), false);

            if (schema != null)
            {
                var discriminator = new OpenApiDiscriminator
                {
                    PropertyName = "actionType"
                };

                schema.DiscriminatorObject = discriminator;

                schema.Properties[discriminator.PropertyName] = new JsonSchemaProperty
                {
                    Type = JsonObjectType.String,
                    IsRequired = true,
                    IsNullableRaw = true
                };

                // The types come from another assembly so we have to fix it here.
                foreach (var (key, value) in ruleRegistry.Actions)
                {
                    var derivedType = $"{key}RuleActionDto";
                    var derivedSchema = context.SchemaGenerator.Generate<JsonSchema>(value.Type.ToContextualType(), context.SchemaResolver);

                    var oldName = context.Document.Definitions.FirstOrDefault(x => x.Value == derivedSchema).Key;

                    if (oldName != null)
                    {
                        context.Document.Definitions.Remove(oldName);
                        context.Document.Definitions.Add(derivedType, derivedSchema);
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

    private static void RemoveFreezable(DocumentProcessorContext context, JsonSchema schema)
    {
        context.Document.Definitions.Remove("Freezable");

        schema.AllOf.Clear();
    }
}
