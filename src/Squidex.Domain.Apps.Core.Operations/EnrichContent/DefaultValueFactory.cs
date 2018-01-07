// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.EnrichContent
{
    public sealed class DefaultValueFactory : IFieldPropertiesVisitor<JToken>
    {
        private readonly Instant now;

        private DefaultValueFactory(Instant now)
        {
            this.now = now;
        }

        public static JToken CreateDefaultValue(Field field, Instant now)
        {
            Guard.NotNull(field, nameof(field));

            return field.RawProperties.Accept(new DefaultValueFactory(now));
        }

        public JToken Visit(AssetsFieldProperties properties)
        {
            return new JArray();
        }

        public JToken Visit(BooleanFieldProperties properties)
        {
            return properties.DefaultValue;
        }

        public JToken Visit(GeolocationFieldProperties properties)
        {
            return JValue.CreateNull();
        }

        public JToken Visit(JsonFieldProperties properties)
        {
            return JValue.CreateNull();
        }

        public JToken Visit(NumberFieldProperties properties)
        {
            return properties.DefaultValue;
        }

        public JToken Visit(ReferencesFieldProperties properties)
        {
            return new JArray();
        }

        public JToken Visit(StringFieldProperties properties)
        {
            return properties.DefaultValue;
        }

        public JToken Visit(TagsFieldProperties properties)
        {
            return new JArray();
        }

        public JToken Visit(DateTimeFieldProperties properties)
        {
            if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return now.ToString();
            }

            if (properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return properties.DefaultValue?.ToString();
        }
    }
}
