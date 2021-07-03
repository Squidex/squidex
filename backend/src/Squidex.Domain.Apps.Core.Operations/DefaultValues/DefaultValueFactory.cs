// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.DefaultValues
{
    public sealed class DefaultValueFactory : IFieldPropertiesVisitor<IJsonValue, DefaultValueFactory.Args>
    {
        private static readonly DefaultValueFactory Instance = new DefaultValueFactory();

        public sealed record Args(Instant Now, string Partition);

        private DefaultValueFactory()
        {
        }

        public static IJsonValue CreateDefaultValue(IField field, Instant now, string partition)
        {
            Guard.NotNull(field, nameof(field));
            Guard.NotNull(partition, nameof(partition));

            return field.RawProperties.Accept(Instance, new Args(now, partition));
        }

        public IJsonValue Visit(ArrayFieldProperties properties, Args args)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(AssetsFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return Array(value);
        }

        public IJsonValue Visit(BooleanFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return JsonValue.Create(value);
        }

        public IJsonValue Visit(ComponentFieldProperties properties, Args args)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(ComponentsFieldProperties properties, Args args)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(GeolocationFieldProperties properties, Args args)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(JsonFieldProperties properties, Args args)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(NumberFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return JsonValue.Create(value);
        }

        public IJsonValue Visit(ReferencesFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return Array(value);
        }

        public IJsonValue Visit(StringFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return JsonValue.Create(value);
        }

        public IJsonValue Visit(TagsFieldProperties properties, Args args)
        {
            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return Array(value);
        }

        public IJsonValue Visit(UIFieldProperties properties, Args args)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(DateTimeFieldProperties properties, Args args)
        {
            if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return JsonValue.Create(args.Now);
            }

            if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return JsonValue.Create($"{args.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T00:00:00Z");
            }

            var value = GetDefaultValue(properties.DefaultValue, properties.DefaultValues, args.Partition);

            return JsonValue.Create(value);
        }

        private static T GetDefaultValue<T>(T value, LocalizedValue<T>? values, string partition)
        {
            if (values != null && values.TryGetValue(partition, out var @default))
            {
                return @default;
            }

            return value;
        }

        private static IJsonValue Array(IEnumerable<string>? values)
        {
            if (values != null)
            {
                return JsonValue.Array(values);
            }
            else
            {
                return JsonValue.Array();
            }
        }
    }
}
