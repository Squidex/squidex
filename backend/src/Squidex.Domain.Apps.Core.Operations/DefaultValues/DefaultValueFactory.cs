// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.DefaultValues
{
    public sealed class DefaultValueFactory : IFieldVisitor<IJsonValue>
    {
        private readonly Instant now;
        private readonly string partition;

        private DefaultValueFactory(Instant now, string partition)
        {
            this.now = now;
            this.partition = partition;
        }

        public static IJsonValue CreateDefaultValue(IField field, Instant now, string partition)
        {
            Guard.NotNull(field, nameof(field));

            return field.Accept(new DefaultValueFactory(now, partition));
        }

        public IJsonValue Visit(IArrayField field)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(IField<AssetsFieldProperties> field)
        {
            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return Array(value);
        }

        public IJsonValue Visit(IField<BooleanFieldProperties> field)
        {
            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return JsonValue.Create(value);
        }

        public IJsonValue Visit(IField<GeolocationFieldProperties> field)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(IField<JsonFieldProperties> field)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(IField<NumberFieldProperties> field)
        {
            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return JsonValue.Create(value);
        }

        public IJsonValue Visit(IField<ReferencesFieldProperties> field)
        {
            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return Array(value);
        }

        public IJsonValue Visit(IField<StringFieldProperties> field)
        {
            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return JsonValue.Create(value);
        }

        public IJsonValue Visit(IField<TagsFieldProperties> field)
        {
            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return Array(value);
        }

        public IJsonValue Visit(IField<UIFieldProperties> field)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(IField<DateTimeFieldProperties> field)
        {
            if (field.Properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return JsonValue.Create(now);
            }

            if (field.Properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return JsonValue.Create($"{now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T00:00:00Z");
            }

            var value = GetDefaultValue(field.Properties.DefaultValue, field.Properties.DefaultValues);

            return JsonValue.Create(value);
        }

        private T GetDefaultValue<T>(T value, LocalizedValue<T>? values)
        {
            if (values != null && values.TryGetValue(partition, out var @default))
            {
                return @default;
            }

            return value;
        }

        private static IJsonValue Array(string[]? values)
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
