// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public sealed class StringFormatter : IFieldPropertiesVisitor<string, StringFormatter.Args>
    {
        private static readonly StringFormatter Instance = new StringFormatter();

        public record struct Args(JsonValue2 Value);

        private StringFormatter()
        {
        }

        public static string Format(IField field, JsonValue2 value)
        {
            Guard.NotNull(field);

            if (value.Type == JsonValueType.Null)
            {
                return string.Empty;
            }

            var args = new Args(value);

            return field.RawProperties.Accept(Instance, args);
        }

        public string Visit(ArrayFieldProperties properties, Args args)
        {
            return FormatArray(args.Value, "Item", "Items");
        }

        public string Visit(AssetsFieldProperties properties, Args args)
        {
            return FormatArray(args.Value, "Asset", "Assets");
        }

        public string Visit(BooleanFieldProperties properties, Args args)
        {
            if (args.Value.Type == JsonValueType.Boolean && args.Value.AsBoolean)
            {
                return "Yes";
            }
            else
            {
                return "No";
            }
        }

        public string Visit(ComponentFieldProperties properties, Args args)
        {
            return "{ Component }";
        }

        public string Visit(ComponentsFieldProperties properties, Args args)
        {
            return FormatArray(args.Value, "Component", "Components");
        }

        public string Visit(DateTimeFieldProperties properties, Args args)
        {
            return args.Value.ToString();
        }

        public string Visit(GeolocationFieldProperties properties, Args args)
        {
            if (args.Value.Type == JsonValueType.Number &&
                args.Value.TryGetValue(JsonValueType.Number, "latitude", out var lat) &&
                args.Value.TryGetValue(JsonValueType.Number, "longitude", out var lon))
            {
                return $"{lat.AsNumber}, {lon.AsNumber}";
            }
            else
            {
                return string.Empty;
            }
        }

        public string Visit(JsonFieldProperties properties, Args args)
        {
            return "<Json />";
        }

        public string Visit(NumberFieldProperties properties, Args args)
        {
            return args.Value.ToString();
        }

        public string Visit(ReferencesFieldProperties properties, Args args)
        {
            return FormatArray(args.Value, "Reference", "References");
        }

        public string Visit(StringFieldProperties properties, Args args)
        {
            if (properties.Editor == StringFieldEditor.StockPhoto)
            {
                return "[Photo]";
            }
            else
            {
                return args.Value.ToString();
            }
        }

        public string Visit(TagsFieldProperties properties, Args args)
        {
            if (args.Value.Type == JsonValueType.Array)
            {
                return string.Join(", ", args.Value.AsArray);
            }
            else
            {
                return string.Empty;
            }
        }

        public string Visit(UIFieldProperties properties, Args args)
        {
            return string.Empty;
        }

        private static string FormatArray(JsonValue2 value, string singularName, string pluralName)
        {
            if (value.Type == JsonValueType.Array)
            {
                var array = value.AsArray;

                if (array.Count > 1)
                {
                    return $"{array.Count} {pluralName}";
                }
                else if (array.Count == 1)
                {
                    return $"1 {singularName}";
                }
            }

            return $"0 {pluralName}";
        }
    }
}
