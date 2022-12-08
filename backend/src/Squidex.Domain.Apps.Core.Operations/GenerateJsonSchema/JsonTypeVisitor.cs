// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json;
using Squidex.Text;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema;

internal sealed class JsonTypeVisitor : IFieldVisitor<JsonSchemaProperty?, JsonTypeVisitor.Args>
{
    private const int MaxDepth = 5;
    private static readonly JsonTypeVisitor Instance = new JsonTypeVisitor();

    public record struct Args(ResolvedComponents Components, Schema Schema,
        JsonTypeFactory Factory,
        bool WithHidden,
        bool WithComponents,
        int Level = 0);

    private JsonTypeVisitor()
    {
    }

    public static JsonSchemaProperty? BuildProperty(IField field, ResolvedComponents components, Schema schema,
        JsonTypeFactory factory,
        bool withHidden,
        bool withComponents)
    {
        var args = new Args(components, schema, factory, withHidden, withComponents);

        return field.Accept(Instance, args);
    }

    private JsonSchemaProperty? Accept(Args args, NestedField nestedField)
    {
        if (args.Level > MaxDepth)
        {
            return null;
        }

        return nestedField.Accept(this, args);
    }

    public JsonSchemaProperty? Visit(IArrayField field, Args args)
    {
        // Create a reference to give it a nice name in code generation.
        var (reference, actual) = args.Factory($"{args.Schema.TypeName()}{field.Name.ToPascalCase()}ItemDto");

        if (actual != null)
        {
            var nestedArgs = args with { Level = args.Level + 1 };

            foreach (var nestedField in field.Fields.ForApi(args.WithHidden))
            {
                var nestedProperty = Accept(nestedArgs, nestedField);

                if (nestedProperty != null)
                {
                    nestedProperty.Description = nestedField.RawProperties.Hints;
                    nestedProperty.SetRequired(nestedField.RawProperties.IsRequired);

                    actual.Properties.Add(nestedField.Name, nestedProperty);
                }
            }
        }

        return JsonTypeBuilder.ArrayProperty(reference);
    }

    public JsonSchemaProperty? Visit(IField<AssetsFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.ArrayProperty(JsonTypeBuilder.String());

        property.Default = field.Properties.DefaultValue;

        if (field.Properties.MinItems != null)
        {
            property.MinItems = field.Properties.MinItems.Value;
        }

        if (field.Properties.MaxItems != null)
        {
            property.MaxItems = field.Properties.MaxItems.Value;
        }

        return property;
    }

    public JsonSchemaProperty? Visit(IField<BooleanFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.BooleanProperty();

        property.Default = field.Properties.DefaultValue;

        return property;
    }

    public JsonSchemaProperty? Visit(IField<ComponentFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.ObjectProperty();

        BuildComponent(property, field.Properties.SchemaIds, args);

        return property;
    }

    public JsonSchemaProperty? Visit(IField<ComponentsFieldProperties> field, Args args)
    {
        var itemSchema = JsonTypeBuilder.Object();

        BuildComponent(itemSchema, field.Properties.SchemaIds, args);

        var property = JsonTypeBuilder.ArrayProperty(itemSchema);

        if (field.Properties.MinItems != null)
        {
            property.MinItems = field.Properties.MinItems.Value;
        }

        if (field.Properties.MaxItems != null)
        {
            property.MaxItems = field.Properties.MaxItems.Value;
        }

        return property;
    }

    public JsonSchemaProperty? Visit(IField<DateTimeFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.DateTimeProperty();

        property.Default = field.Properties.DefaultValue?.ToString();

        return property;
    }

    public JsonSchemaProperty? Visit(IField<GeolocationFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.ObjectProperty();

        property.Format = GeoJson.Format;

        return property;
    }

    public JsonSchemaProperty? Visit(IField<JsonFieldProperties> field, Args args)
    {
        return JsonTypeBuilder.JsonProperty();
    }

    public JsonSchemaProperty? Visit(IField<NumberFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.NumberProperty();

        property.Default = field.Properties.DefaultValue;

        if (field.Properties.MinValue != null)
        {
            property.Minimum = (decimal)field.Properties.MinValue.Value;
        }

        if (field.Properties.MaxValue != null)
        {
            property.Maximum = (decimal)field.Properties.MaxValue.Value;
        }

        return property;
    }

    public JsonSchemaProperty? Visit(IField<ReferencesFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.ArrayProperty(JsonTypeBuilder.String());

        property.Default = field.Properties.DefaultValue;

        if (field.Properties.MinItems != null)
        {
            property.MinItems = field.Properties.MinItems.Value;
        }

        if (field.Properties.MaxItems != null)
        {
            property.MaxItems = field.Properties.MaxItems.Value;
        }

        property.ExtensionData = new Dictionary<string, object>
        {
            ["schemaIds"] = field.Properties.SchemaIds ?? ReadonlyList.Empty<DomainId>()
        };

        property.UniqueItems = !field.Properties.AllowDuplicates;

        return property;
    }

    public JsonSchemaProperty? Visit(IField<StringFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.StringProperty();

        property.Default = field.Properties.DefaultValue;

        property.MaxLength = field.Properties.MaxLength;
        property.MinLength = field.Properties.MinLength;

        property.Pattern = field.Properties.Pattern;

        if (field.Properties.AllowedValues != null)
        {
            var names = property.EnumerationNames ??= new Collection<string>();

            foreach (var value in field.Properties.AllowedValues)
            {
                names.Add(value);
            }
        }

        return property;
    }

    public JsonSchemaProperty? Visit(IField<TagsFieldProperties> field, Args args)
    {
        var property = JsonTypeBuilder.ArrayProperty(JsonTypeBuilder.String());

        property.Default = field.Properties.DefaultValue;

        if (field.Properties.MinItems != null)
        {
            property.MinItems = field.Properties.MinItems.Value;
        }

        if (field.Properties.MaxItems != null)
        {
            property.MaxItems = field.Properties.MaxItems.Value;
        }

        return property;
    }

    public JsonSchemaProperty? Visit(IField<UIFieldProperties> field, Args args)
    {
        return null;
    }

    private static void BuildComponent(JsonSchema jsonSchema, ReadonlyList<DomainId>? schemaIds, Args args)
    {
        if (args.WithComponents)
        {
            var discriminator = new OpenApiDiscriminator
            {
                PropertyName = Component.Discriminator
            };

            foreach (var schema in args.Components.Resolve(schemaIds).Values)
            {
                // Create a reference to give it a nice name in code generation.
                var (reference, actual) = args.Factory($"{schema.TypeName()}ComponentDto");

                if (actual != null)
                {
                    foreach (var field in schema.Fields.ForApi(args.WithHidden))
                    {
                        var property =
                            BuildProperty(
                                field,
                                args.Components,
                                schema,
                                args.Factory,
                                args.WithHidden,
                                args.WithComponents);

                        if (property != null)
                        {
                            property.SetRequired(field.RawProperties.IsRequired);
                            property.SetDescription(field);

                            actual.Properties.Add(field.Name, property);
                        }
                    }
                }

                jsonSchema.OneOf.Add(reference);

                discriminator.Mapping[schema.Name] = reference;
            }

            jsonSchema.DiscriminatorObject = discriminator;

            if (discriminator.Mapping.Count > 0)
            {
                jsonSchema.Properties.Add(Component.Discriminator, JsonTypeBuilder.StringProperty(isRequired: true));
            }
        }
        else
        {
            jsonSchema.AllowAdditionalProperties = true;
        }
    }
}
