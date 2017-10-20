﻿// ==========================================================================
//  DateTimeField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class DateTimeField : Field<DateTimeFieldProperties>
    {
        public DateTimeField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new DateTimeFieldProperties())
        {
        }

        public DateTimeField(long id, string name, Partitioning partitioning, DateTimeFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }

            if (Properties.MinValue.HasValue || Properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<Instant>(Properties.MinValue, Properties.MaxValue);
            }
        }

        public override object ConvertValue(JToken value)
        {
            if (value.Type == JTokenType.String)
            {
                var parseResult = InstantPattern.General.Parse(value.ToString());

                if (!parseResult.Success)
                {
                    throw parseResult.Exception;
                }

                return parseResult.Value;
            }

            throw new InvalidCastException("Invalid json type, expected string.");
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
