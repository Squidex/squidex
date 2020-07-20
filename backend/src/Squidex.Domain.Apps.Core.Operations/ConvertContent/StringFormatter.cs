// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public sealed class StringFormatter : IFieldVisitor<string>
    {
        private readonly IJsonValue value;

        private StringFormatter(IJsonValue value)
        {
            this.value = value;
        }

        public static string Format(IJsonValue? value, IField field)
        {
            Guard.NotNull(field, nameof(field));

            if (value == null || value is JsonNull)
            {
                return string.Empty;
            }

            return field.Accept(new StringFormatter(value));
        }

        public string Visit(IArrayField field)
        {
            return FormatArray("Item", "Items");
        }

        public string Visit(IField<AssetsFieldProperties> field)
        {
            return FormatArray("Asset", "Assets");
        }

        public string Visit(IField<BooleanFieldProperties> field)
        {
            if (value is JsonBoolean boolean && boolean.Value)
            {
                return "Yes";
            }
            else
            {
                return "No";
            }
        }

        public string Visit(IField<DateTimeFieldProperties> field)
        {
            return value.ToString();
        }

        public string Visit(IField<GeolocationFieldProperties> field)
        {
            if (value is JsonObject obj && obj.TryGetValue("latitude", out var lat) && obj.TryGetValue("longitude", out var lon))
            {
                return $"{lat}, {lon}";
            }
            else
            {
                return string.Empty;
            }
        }

        public string Visit(IField<JsonFieldProperties> field)
        {
            return "<Json />";
        }

        public string Visit(IField<NumberFieldProperties> field)
        {
            return value.ToString();
        }

        public string Visit(IField<ReferencesFieldProperties> field)
        {
            return FormatArray("Reference", "References");
        }

        public string Visit(IField<StringFieldProperties> field)
        {
            if (field.Properties.Editor == StringFieldEditor.StockPhoto)
            {
                return "[Photo]";
            }
            else
            {
                return value.ToString();
            }
        }

        public string Visit(IField<TagsFieldProperties> field)
        {
            if (value is JsonArray array)
            {
                return string.Join(", ", array);
            }
            else
            {
                return string.Empty;
            }
        }

        public string Visit(IField<UIFieldProperties> field)
        {
            return string.Empty;
        }

        private string FormatArray(string singularName, string pluralName)
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
