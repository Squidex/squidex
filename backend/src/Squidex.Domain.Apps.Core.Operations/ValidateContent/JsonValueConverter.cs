// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueConverter : IFieldVisitor<(object? Result, JsonError? Error), JsonValueConverter.Args>
    {
        private static readonly JsonValueConverter Instance = new JsonValueConverter();

        public record struct Args(JsonValue Value, IJsonSerializer JsonSerializer, ResolvedComponents Components);

        private JsonValueConverter()
        {
        }

        public static (object? Result, JsonError? Error) ConvertValue(IField field, JsonValue value, IJsonSerializer jsonSerializer,
            ResolvedComponents components)
        {
            Guard.NotNull(field);
            Guard.NotNull(value);

            var args = new Args(value, jsonSerializer, components);

            return field.Accept(Instance, args);
        }

        public (object? Result, JsonError? Error) Visit(IField<JsonFieldProperties> field, Args args)
        {
            return (args.Value, null);
        }

        public (object? Result, JsonError? Error) Visit(IArrayField field, Args args)
        {
            return ConvertToObjectList(args.Value);
        }

        public (object? Result, JsonError? Error) Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return ConvertToIdList(args.Value);
        }

        public (object? Result, JsonError? Error) Visit(IField<ComponentFieldProperties> field, Args args)
        {
            return ConvertToComponent(args.Value, args.Components, field.Properties.SchemaIds);
        }

        public (object? Result, JsonError? Error) Visit(IField<ComponentsFieldProperties> field, Args args)
        {
            return ConvertToComponentList(args.Value, args.Components, field.Properties.SchemaIds);
        }

        public (object? Result, JsonError? Error) Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return ConvertToIdList(args.Value);
        }

        public (object? Result, JsonError? Error) Visit(IField<TagsFieldProperties> field, Args args)
        {
            return ConvertToStringList(args.Value);
        }

        public (object? Result, JsonError? Error) Visit(IField<BooleanFieldProperties> field, Args args)
        {
            if (args.Value.Type == JsonValueType.Boolean)
            {
                return (args.Value.AsBoolean, null);
            }

            return (null, new JsonError(T.Get("contents.invalidBoolean")));
        }

        public (object? Result, JsonError? Error) Visit(IField<NumberFieldProperties> field, Args args)
        {
            if (args.Value.Type == JsonValueType.Number)
            {
                return (args.Value.AsNumber, null);
            }

            return (null, new JsonError(T.Get("contents.invalidNumber")));
        }

        public (object? Result, JsonError? Error) Visit(IField<StringFieldProperties> field, Args args)
        {
            if (args.Value.Type == JsonValueType.String)
            {
                return (args.Value.AsString, null);
            }

            return (null, new JsonError(T.Get("contents.invalidString")));
        }

        public (object? Result, JsonError? Error) Visit(IField<UIFieldProperties> field, Args args)
        {
            return (args.Value, null);
        }

        public (object? Result, JsonError? Error) Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            if (args.Value.Type == JsonValueType.String)
            {
                var parseResult = InstantPattern.ExtendedIso.Parse(args.Value.ToString());

                if (!parseResult.Success)
                {
                    return (null, new JsonError(parseResult.Exception.Message));
                }

                return (parseResult.Value, null);
            }

            return (null, new JsonError(T.Get("contents.invalidString")));
        }

        public (object? Result, JsonError? Error) Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            var result = GeoJsonValue.TryParse(args.Value, args.JsonSerializer, out var value);

            switch (result)
            {
                case GeoJsonParseResult.InvalidLatitude:
                    return (null, new JsonError(T.Get("contents.invalidGeolocationLatitude")));
                case GeoJsonParseResult.InvalidLongitude:
                    return (null, new JsonError(T.Get("contents.invalidGeolocationLongitude")));
                case GeoJsonParseResult.InvalidValue:
                    return (null, new JsonError(T.Get("contents.invalidGeolocation")));
                default:
                    return (value, null);
            }
        }

        private static (object? Result, JsonError? Error) ConvertToIdList(JsonValue value)
        {
            if (value.Type == JsonValueType.Array)
            {
                var array = value.AsArray;

                var result = new List<DomainId>(array.Count);

                foreach (var item in array)
                {
                    if (item.Type == JsonValueType.String)
                    {
                        var typed = item.AsString;

                        if (!string.IsNullOrWhiteSpace(item.AsString))
                        {
                            result.Add(DomainId.Create(typed));
                            continue;
                        }
                    }

                    return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
                }

                return (result, null);
            }

            return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
        }

        private static (object? Result, JsonError? Error) ConvertToStringList(JsonValue value)
        {
            if (value.Type == JsonValueType.Array)
            {
                var array = value.AsArray;

                var result = new List<string?>(array.Count);

                foreach (var item in array)
                {
                    if (item.Type == JsonValueType.String)
                    {
                        var typed = item.AsString;

                        if (!string.IsNullOrWhiteSpace(item.AsString))
                        {
                            result.Add(typed);
                            continue;
                        }
                    }

                    return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
                }

                return (result, null);
            }

            return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
        }

        private static (object? Result, JsonError? Error) ConvertToObjectList(JsonValue value)
        {
            if (value.Type == JsonValueType.Array)
            {
                var array = value.AsArray;

                var result = new List<JsonObject>(array.Count);

                foreach (var item in array)
                {
                    if (item.Type == JsonValueType.Object)
                    {
                        result.Add(item.AsObject);
                        continue;
                    }

                    return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
                }

                return (result, null);
            }

            return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
        }

        private static (object? Result, JsonError? Error) ConvertToComponentList(JsonValue value,
            ResolvedComponents components, ReadonlyList<DomainId>? allowedIds)
        {
            if (value.Type == JsonValueType.Array)
            {
                var array = value.AsArray;

                var result = new List<Component>(array.Count);

                foreach (var item in array)
                {
                    var (component, error) = ConvertToComponent(item, components, allowedIds);

                    if (error != null)
                    {
                        return (null, error);
                    }

                    if (component != null)
                    {
                        result.Add(component);
                    }
                }

                return (result, null);
            }

            return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
        }

        private static (Component? Result, JsonError? Error) ConvertToComponent(JsonValue value,
            ResolvedComponents components, ReadonlyList<DomainId>? allowedIds)
        {
            if (value.Type != JsonValueType.Object)
            {
                return (null, new JsonError(T.Get("contents.invalidComponentNoObject")));
            }

            var id = default(DomainId);

            var obj = value.AsObject;

            if (obj.TryGetValue("schemaName", out var schemaName) && schemaName.Type == JsonValueType.String)
            {
                id = components.FirstOrDefault(x => x.Value.Name == schemaName.AsString).Key;

                obj.Remove("schemaName");
                obj[Component.Discriminator] = id;
            }
            else if (obj.TryGetValue(Component.Discriminator, out var discriminator) && discriminator.Type == JsonValueType.String)
            {
                id = DomainId.Create(discriminator.AsString);
            }
            else if (allowedIds?.Count == 1)
            {
                id = allowedIds[0];

                obj[Component.Discriminator] = id;
            }

            if (id == default)
            {
                return (null, new JsonError(T.Get("contents.invalidComponentNoType")));
            }

            if (allowedIds?.Contains(id) == false || !components.TryGetValue(id, out var schema))
            {
                return (null, new JsonError(T.Get("contents.invalidComponentUnknownSchema")));
            }

            var data = new JsonObject(obj);

            data.Remove(Component.Discriminator);

            return (new Component(id.ToString(), data, schema), null);
        }
    }
}
