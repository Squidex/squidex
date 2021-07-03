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

        public sealed record Args(IJsonValue Value);

        private StringFormatter()
        {
        }

        public static string Format(IField field, IJsonValue? value)
        {
            Guard.NotNull(field, nameof(field));

            if (value == null || value is JsonNull)
            {
                return string.Empty;
            }

            var args = new Args(value ?? JsonValue.Null);

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
            if (args.Value is JsonBoolean { Value: true })
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
            if (args.Value is JsonObject jsonObject &&
                jsonObject.TryGetValue<JsonNumber>("latitude", out var lat) &&
                jsonObject.TryGetValue<JsonNumber>("longitude", out var lon))
            {
                return $"{lat}, {lon}";
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
            if (args.Value is JsonArray array)
            {
                return string.Join(", ", array);
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

        private static string FormatArray(IJsonValue value, string singularName, string pluralName)
        {
            if (value is JsonArray array)
            {
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
