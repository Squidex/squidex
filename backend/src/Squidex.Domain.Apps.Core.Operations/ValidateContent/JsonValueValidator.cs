﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonValueValidator : IFieldVisitor<bool, JsonValueValidator.Args>
    {
        private static readonly JsonValueValidator Instance = new JsonValueValidator();

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

        private JsonValueValidator()
        {
        }

        public static bool IsValid(IField field, IJsonValue value, IJsonSerializer jsonSerializer)
        {
            Guard.NotNull(field, nameof(field));
            Guard.NotNull(value, nameof(value));

            var args = new Args(value, jsonSerializer);

            return field.Accept(Instance, args);
        }

        public bool Visit(IArrayField field, Args args)
        {
            return IsValidObjectList(args.Value);
        }

        public bool Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return IsValidStringList(args.Value);
        }

        public bool Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return IsValidStringList(args.Value);
        }

        public bool Visit(IField<TagsFieldProperties> field, Args args)
        {
            return IsValidStringList(args.Value);
        }

        public bool Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return args.Value is JsonBoolean;
        }

        public bool Visit(IField<NumberFieldProperties> field, Args args)
        {
            return args.Value is JsonNumber;
        }

        public bool Visit(IField<StringFieldProperties> field, Args args)
        {
            return args.Value is JsonString;
        }

        public bool Visit(IField<UIFieldProperties> field, Args args)
        {
            return true;
        }

        public bool Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            if (args.Value.Type == JsonValueType.String)
            {
                var parseResult = InstantPattern.General.Parse(args.Value.ToString());

                return parseResult.Success;
            }

            return false;
        }

        public bool Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            var result = GeoJsonValue.TryParse(args.Value, args.JsonSerializer, out _);

            return result == GeoJsonParseResult.Success;
        }

        public bool Visit(IField<JsonFieldProperties> field, Args args)
        {
            return true;
        }

        private static bool IsValidStringList(IJsonValue value)
        {
            if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item is not JsonString)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static bool IsValidObjectList(IJsonValue value)
        {
            if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item is not JsonObject)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
