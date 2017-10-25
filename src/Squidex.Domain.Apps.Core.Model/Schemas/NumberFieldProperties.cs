// ==========================================================================
//  NumberFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(NumberField))]
    public sealed class NumberFieldProperties : FieldProperties
    {
        public double? MaxValue { get; set; }

        public double? MinValue { get; set; }

        public double? DefaultValue { get; set; }

        public double[] AllowedValues { get; set; }

        public NumberFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
