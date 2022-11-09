// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.GenerateFilters;

internal sealed class FilterVisitor : IFieldVisitor<FilterSchema?, FilterVisitor.Args>
{
    private const int MaxDepth = 3;
    private static readonly FilterVisitor Instance = new FilterVisitor();

    public record struct Args(ResolvedComponents Components, int Level = 0);

    private FilterVisitor()
    {
    }

    public static FilterSchema? BuildProperty(IField field, ResolvedComponents components)
    {
        var args = new Args(components);

        return field.Accept(Instance, args);
    }

    public FilterSchema? Visit(IArrayField field, Args args)
    {
        if (args.Level >= MaxDepth)
        {
            return null;
        }

        var fields = new List<FilterField>();

        var nestedArgs = args with { Level = args.Level + 1 };

        foreach (var nestedField in field.Fields.ForApi(true))
        {
            var nestedSchema = nestedField.Accept(this, nestedArgs);

            if (nestedSchema != null)
            {
                var filterableField = new FilterField(
                    nestedSchema,
                    nestedField.Name,
                    ArrayFieldDescription(nestedField),
                    true);

                fields.Add(filterableField);
            }
        }

        return new FilterSchema(FilterSchemaType.ObjectArray)
        {
            Fields = fields.ToReadonlyList()
        };
    }

    public FilterSchema? Visit(IField<AssetsFieldProperties> field, Args args)
    {
        return FilterSchema.StringArray;
    }

    public FilterSchema? Visit(IField<BooleanFieldProperties> field, Args args)
    {
        return FilterSchema.Boolean;
    }

    public FilterSchema? Visit(IField<GeolocationFieldProperties> field, Args args)
    {
        return FilterSchema.GeoObject;
    }

    public FilterSchema? Visit(IField<JsonFieldProperties> field, Args args)
    {
        return FilterSchema.Any;
    }

    public FilterSchema? Visit(IField<NumberFieldProperties> field, Args args)
    {
        return FilterSchema.Number;
    }

    public FilterSchema? Visit(IField<StringFieldProperties> field, Args args)
    {
        if (field.Properties.AllowedValues?.Count > 0)
        {
            return new FilterSchema(FilterSchemaType.String)
            {
                Extra = new
                {
                    options = field.Properties.AllowedValues
                }
            };
        }

        return FilterSchema.String;
    }

    public FilterSchema? Visit(IField<TagsFieldProperties> field, Args args)
    {
        return FilterSchema.StringArray;
    }

    public FilterSchema? Visit(IField<UIFieldProperties> field, Args args)
    {
        return null;
    }

    public FilterSchema? Visit(IField<ComponentFieldProperties> field, Args args)
    {
        if (args.Level >= MaxDepth)
        {
            return null;
        }

        return new FilterSchema(FilterSchemaType.Object)
        {
            Fields = BuildComponent(field.Properties.SchemaIds, args)
        };
    }

    public FilterSchema? Visit(IField<ComponentsFieldProperties> field, Args args)
    {
        if (args.Level >= MaxDepth)
        {
            return null;
        }

        return new FilterSchema(FilterSchemaType.Object)
        {
            Fields = BuildComponent(field.Properties.SchemaIds, args)
        };
    }

    public FilterSchema? Visit(IField<DateTimeFieldProperties> field, Args args)
    {
        if (field.Properties.Editor == DateTimeFieldEditor.Date)
        {
            return SharedSchemas.Date;
        }

        return SharedSchemas.DateTime;
    }

    public FilterSchema? Visit(IField<ReferencesFieldProperties> field, Args args)
    {
        return new FilterSchema(FilterSchemaType.StringArray)
        {
            Extra = new
            {
                schemaIds = field.Properties.SchemaIds
            }
        };
    }

    private ReadonlyList<FilterField> BuildComponent(ReadonlyList<DomainId>? schemaIds, Args args)
    {
        var fields = new List<FilterField>();

        var nestedArgs = args with { Level = args.Level + 1 };

        foreach (var (_, schema) in args.Components.Resolve(schemaIds))
        {
            var componentName = schema.DisplayName();

            foreach (var field in schema.Fields.ForApi(true))
            {
                var fieldSchema = field.Accept(this, nestedArgs);

                if (fieldSchema != null)
                {
                    var filterableField = new FilterField(
                        fieldSchema,
                        field.Name,
                        ComponentFieldDescription(componentName, field),
                        true);

                    fields.Add(filterableField);
                }
            }

            fields.Add(new FilterField(FilterSchema.String, Component.Discriminator)
            {
                Description = FieldDescriptions.ComponentSchemaId
            });
        }

        return fields.ToReadonlyList();
    }

    private static string ArrayFieldDescription(IField field)
    {
        var name = field.DisplayName();

        return string.Format(CultureInfo.InvariantCulture, FieldDescriptions.ContentArrayField, name);
    }

    private static string ComponentFieldDescription(string componentName, RootField field)
    {
        var name = field.DisplayName();

        return string.Format(CultureInfo.InvariantCulture, FieldDescriptions.ComponentField, name, componentName);
    }
}
