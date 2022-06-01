// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using System.Globalization;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.DefaultValues
{
    public sealed class DefaultValueFactory : IFieldPropertiesVisitor<JsonValue2, DefaultValueFactory.Args>
    {
        private static readonly DefaultValueFactory Instance = new DefaultValueFactory();

        public record struct Args(Instant Now, string Partition);

        private DefaultValueFactory()
        {
        }

        public static JsonValue2 CreateDefaultValue(IField field, Instant now, string partition)
        {
            Guard.NotNull(field);
            Guard.NotNull(partition);

            return field.RawProperties.Accept(Instance, new Args(now, partition));
        }

        public JsonValue2 Visit(ArrayFieldProperties properties, Args args)
        {
            return JsonValue2.Array();
        }

        public JsonValue2 Visit(AssetsFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return Array(value);
        }

        public JsonValue2 Visit(BooleanFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return value != null && value.Value;
        }

        public JsonValue2 Visit(ComponentFieldProperties properties, Args args)
        {
            return default;
        }

        public JsonValue2 Visit(ComponentsFieldProperties properties, Args args)
        {
            return JsonValue2.Array();
        }

        public JsonValue2 Visit(GeolocationFieldProperties properties, Args args)
        {
            return default;
        }

        public JsonValue2 Visit(JsonFieldProperties properties, Args args)
        {
            return default;
        }

        public JsonValue2 Visit(NumberFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return value ?? default;
        }

        public JsonValue2 Visit(ReferencesFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return Array(value);
        }

        public JsonValue2 Visit(StringFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return value;
        }

        public JsonValue2 Visit(TagsFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return Array(value);
        }

        public JsonValue2 Visit(UIFieldProperties properties, Args args)
        {
            return default;
        }

        public JsonValue2 Visit(DateTimeFieldProperties properties, Args args)
        {
            if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return JsonValue2.Create(args.Now);
            }

            if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return JsonValue2.Create($"{args.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T00:00:00Z");
            }

            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return value ?? default;
        }

        private static T GetDefaultValue<T>(T value, LocalizedValue<T>? values, string partition)
        {
            if (values != null && values.TryGetValue(partition, out var @default))
            {
                return @default;
            }

            return value;
        }

        private static JsonValue2 Array(IEnumerable<string>? values)
        {
            if (values != null)
            {
                return JsonValue2.Create(values);
            }
            else
            {
                return JsonValue2.Array();
            }
        }
    }
}
