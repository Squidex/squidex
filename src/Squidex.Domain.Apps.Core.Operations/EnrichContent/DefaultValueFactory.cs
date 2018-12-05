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

namespace Squidex.Domain.Apps.Core.EnrichContent
{
    public sealed class DefaultValueFactory : IFieldVisitor<IJsonValue>
    {
        private readonly Instant now;

        private DefaultValueFactory(Instant now)
        {
            this.now = now;
        }

        public static IJsonValue CreateDefaultValue(IField field, Instant now)
        {
            Guard.NotNull(field, nameof(field));

            return field.Accept(new DefaultValueFactory(now));
        }

        public IJsonValue Visit(IArrayField field)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(IField<AssetsFieldProperties> field)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(IField<BooleanFieldProperties> field)
        {
            return JsonValue.Create(field.Properties.DefaultValue);
        }

        public IJsonValue Visit(IField<GeolocationFieldProperties> field)
        {
            return JsonValue.Null;
        }

        public IJsonValue Visit(IField<JsonFieldProperties> field)
        {
            return JsonValue.Object();
        }

        public IJsonValue Visit(IField<NumberFieldProperties> field)
        {
            return JsonValue.Create(field.Properties.DefaultValue);
        }

        public IJsonValue Visit(IField<ReferencesFieldProperties> field)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(IField<StringFieldProperties> field)
        {
            return JsonValue.Create(field.Properties.DefaultValue);
        }

        public IJsonValue Visit(IField<TagsFieldProperties> field)
        {
            return JsonValue.Array();
        }

        public IJsonValue Visit(IField<DateTimeFieldProperties> field)
        {
            if (field.Properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return JsonValue.Create(now.ToString());
            }

            if (field.Properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return JsonValue.Create(now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            return JsonValue.Create(field.Properties.DefaultValue?.ToString());
        }
    }
}
