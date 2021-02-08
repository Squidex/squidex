// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueConverter : IFieldVisitor<(object? Result, JsonError? Error), JsonValueConverter.Args>
    {
        private static readonly JsonValueConverter Instance = new JsonValueConverter();

        public readonly struct Args
        {
            public readonly IJsonValue Value;
            public readonly IJsonSerializer JsonSerializer;

            public Args(IJsonValue value, IJsonSerializer jsonSerializer)
            {
                Value = value;

                JsonSerializer = jsonSerializer;
            }
        }

        private JsonValueConverter()
        {
        }

        public static (object? Result, JsonError? Error) ConvertValue(IField field, IJsonValue value, IJsonSerializer jsonSerializer)
        {
            Guard.NotNull(field, nameof(field));
            Guard.NotNull(value, nameof(value));

            var args = new Args(value, jsonSerializer);

            return field.Accept(Instance, args);
        }

        public (object? Result, JsonError? Error) Visit(IArrayField field, Args args)
        {
            return ConvertToObjectList(args.Value);
        }

        public (object? Result, JsonError? Error) Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return ConvertToIdList(args.Value);
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
            if (args.Value is JsonBoolean b)
            {
                return (b.Value, null);
            }

            return (null, new JsonError(T.Get("contents.invalidBoolean")));
        }

        public (object? Result, JsonError? Error) Visit(IField<NumberFieldProperties> field, Args args)
        {
            if (args.Value is JsonNumber n)
            {
                return (n.Value, null);
            }

            return (null, new JsonError(T.Get("contents.invalidNumber")));
        }

        public (object? Result, JsonError? Error) Visit(IField<StringFieldProperties> field, Args args)
        {
            if (args.Value is JsonString s)
            {
                return (s.Value, null);
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

        public (object? Result, JsonError? Error) Visit(IField<JsonFieldProperties> field, Args args)
        {
            return (args.Value, null);
        }

        private static (object? Result, JsonError? Error) ConvertToIdList(IJsonValue value)
        {
            if (value is JsonArray array)
            {
                var result = new List<DomainId>(array.Count);

                foreach (var item in array)
                {
                    if (item is JsonString s && !string.IsNullOrWhiteSpace(s.Value))
                    {
                        result.Add(DomainId.Create(s.Value));
                    }
                    else
                    {
                        return (null, new JsonError("Invalid json type, expected array of strings."));
                    }
                }

                return (result, null);
            }

            return (null, new JsonError("Invalid json type, expected array of strings."));
        }

        private static (object? Result, JsonError? Error) ConvertToStringList(IJsonValue value)
        {
            if (value is JsonArray array)
            {
                var result = new List<string?>(array.Count);

                foreach (var item in array)
                {
                    if (item is JsonString s && !string.IsNullOrWhiteSpace(s.Value))
                    {
                        result.Add(s.Value);
                    }
                    else
                    {
                        return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
                    }
                }

                return (result, null);
            }

            return (null, new JsonError(T.Get("contents.invalidArrayOfStrings")));
        }

        private static (object? Result, JsonError? Error) ConvertToObjectList(IJsonValue value)
        {
            if (value is JsonArray array)
            {
                var result = new List<JsonObject>(array.Count);

                foreach (var item in array)
                {
                    if (item is JsonObject obj)
                    {
                        result.Add(obj);
                    }
                    else
                    {
                        return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
                    }
                }

                return (result, null);
            }

            return (null, new JsonError(T.Get("contents.invalidArrayOfObjects")));
        }
    }
}
