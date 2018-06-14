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
    public sealed class DefaultValueFactory : IFieldVisitor<JToken>
    {
        private readonly Instant now;

        private DefaultValueFactory(Instant now)
        {
            this.now = now;
        }

        public static JToken CreateDefaultValue(IField field, Instant now)
        {
            Guard.NotNull(field, nameof(field));

            return field.Accept(new DefaultValueFactory(now));
        }

        public JToken Visit(IArrayField field)
        {
            return new JArray();
        }

        public JToken Visit(IField<AssetsFieldProperties> field)
        {
            return new JArray();
        }

        public JToken Visit(IField<BooleanFieldProperties> field)
        {
            return field.Properties.DefaultValue;
        }

        public JToken Visit(IField<GeolocationFieldProperties> field)
        {
            return JValue.CreateNull();
        }

        public JToken Visit(IField<JsonFieldProperties> field)
        {
            return JValue.CreateNull();
        }

        public JToken Visit(IField<NumberFieldProperties> field)
        {
            return field.Properties.DefaultValue;
        }

        public JToken Visit(IField<ReferencesFieldProperties> field)
        {
            return new JArray();
        }

        public JToken Visit(IField<StringFieldProperties> field)
        {
            return field.Properties.DefaultValue;
        }

        public JToken Visit(IField<TagsFieldProperties> field)
        {
            return new JArray();
        }

        public JToken Visit(IField<DateTimeFieldProperties> field)
        {
            if (field.Properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return now.ToString();
            }

            if (field.Properties.CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return field.Properties.DefaultValue?.ToString();
        }
    }
}
