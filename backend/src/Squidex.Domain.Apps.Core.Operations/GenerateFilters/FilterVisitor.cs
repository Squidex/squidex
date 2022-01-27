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

namespace Squidex.Domain.Apps.Core.GenerateFilters
{
    internal sealed class FilterVisitor : IFieldVisitor<FilterableField?, FilterVisitor.Args>
    {
        private const int MaxDepth = 5;
        private static readonly FilterVisitor Instance = new FilterVisitor();

        public record struct Args(ResolvedComponents Components, int Level = 0);

        private FilterVisitor()
        {
        }

        public static FilterableField? BuildProperty(IField field, ResolvedComponents components)
        {
            var args = new Args(components);

            return field.Accept(Instance, args);
        }

        public FilterableField? Visit(IArrayField field, Args args)
        {
            if (args.Level >= MaxDepth)
            {
                return null;
            }

            var fields = new List<FilterableField>();

            var nestedArgs = args with { Level = args.Level + 1 };

            foreach (var nestedField in field.Fields.ForApi(true))
            {
                var filterableField = nestedField.Accept(this, nestedArgs);

                if (filterableField != null)
                {
                    filterableField = filterableField with
                    {
                        IsNullable = !field.RawProperties.IsRequired,
                        FieldHints = ArrayFieldDescription(nestedField),
                        Fields = filterableField.Fields
                    };

                    fields.Add(filterableField);
                }
            }

            return new FilterableField(FilterableFieldType.ObjectArray, field.Name)
            {
                Fields = fields.ToReadonlyList()
            };
        }

        public FilterableField? Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.StringArray, field.Name);
        }

        public FilterableField? Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.Boolean, field.Name);
        }

        public FilterableField? Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.GeoObject, field.Name);
        }

        public FilterableField? Visit(IField<JsonFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.Any, field.Name);
        }

        public FilterableField? Visit(IField<NumberFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.Number, field.Name);
        }

        public FilterableField? Visit(IField<StringFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.String, field.Name);
        }

        public FilterableField? Visit(IField<TagsFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.StringArray, field.Name);
        }

        public FilterableField? Visit(IField<UIFieldProperties> field, Args args)
        {
            return null;
        }

        public FilterableField? Visit(IField<ComponentFieldProperties> field, Args args)
        {
            if (args.Level >= MaxDepth)
            {
                return null;
            }

            return new FilterableField(FilterableFieldType.Object, field.Name)
            {
                Fields = BuildComponent(field.Properties.SchemaIds, args)
            };
        }

        public FilterableField? Visit(IField<ComponentsFieldProperties> field, Args args)
        {
            if (args.Level >= MaxDepth)
            {
                return null;
            }

            return new FilterableField(FilterableFieldType.ObjectArray, field.Name)
            {
                Fields = BuildComponent(field.Properties.SchemaIds, args)
            };
        }

        public FilterableField? Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.DateTime, field.Name)
            {
                Extra = new
                {
                    editor = field.Properties.Editor.ToString()
                }
            };
        }

        public FilterableField? Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return new FilterableField(FilterableFieldType.StringArray, field.Name)
            {
                Extra = new
                {
                    schemaIds = field.Properties.SchemaIds
                }
            };
        }

        private ReadonlyList<FilterableField> BuildComponent(ReadonlyList<DomainId>? schemaIds, Args args)
        {
            var fields = new List<FilterableField>();

            var nestedArgs = args with { Level = args.Level + 1 };

            foreach (var schema in args.Components.Resolve(schemaIds).Values)
            {
                var componentName = schema.DisplayName();

                foreach (var field in schema.Fields.ForApi(true))
                {
                    var filterableField = field.Accept(this, nestedArgs);

                    if (filterableField != null)
                    {
                        filterableField = filterableField with
                        {
                            IsNullable = !field.RawProperties.IsRequired,
                            FieldHints = ComponentFieldDescription(componentName, field),
                            Fields = filterableField.Fields
                        };

                        fields.Add(filterableField);
                    }
                }

                fields.Add(new FilterableField(FilterableFieldType.String, Component.Discriminator));
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
}
