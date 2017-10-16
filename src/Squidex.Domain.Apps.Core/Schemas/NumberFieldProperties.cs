// ==========================================================================
//  NumberFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(NumberField))]
    public sealed class NumberFieldProperties : FieldProperties
    {
        private double? maxValue;
        private double? minValue;
        private double? defaultValue;
        private ImmutableList<double> allowedValues;
        private NumberFieldEditor editor;

        public double? MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                ThrowIfFrozen();

                maxValue = value;
            }
        }

        public double? MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                ThrowIfFrozen();

                minValue = value;
            }
        }

        public double? DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public ImmutableList<double> AllowedValues
        {
            get
            {
                return allowedValues;
            }
            set
            {
                ThrowIfFrozen();

                allowedValues = value;
            }
        }

        public NumberFieldEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                ThrowIfFrozen();

                editor = value;
            }
        }

        public override JToken GetDefaultValue()
        {
            return DefaultValue;
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
