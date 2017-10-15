// ==========================================================================
//  StringField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class StringField : Field<StringFieldProperties>
    {
        public StringField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new StringFieldProperties())
        {
        }

        public StringField(long id, string name, Partitioning partitioning, StringFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredStringValidator();
            }

            if (Properties.MinLength.HasValue || Properties.MaxLength.HasValue)
            {
                yield return new StringLengthValidator(Properties.MinLength, Properties.MaxLength);
            }

            if (!string.IsNullOrWhiteSpace(Properties.Pattern))
            {
                yield return new PatternValidator(Properties.Pattern, Properties.PatternMessage);
            }

            if (Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<string>(Properties.AllowedValues.ToArray());
            }
        }

        public override object ConvertValue(JToken value)
        {
            return value.ToString();
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
