// ==========================================================================
//  BooleanField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class BooleanField : Field<BooleanFieldProperties>
    {
        public BooleanField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new BooleanFieldProperties())
        {
        }

        public BooleanField(long id, string name, Partitioning partitioning, BooleanFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public override object ConvertValue(JToken value)
        {
            return (bool?)value;
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
