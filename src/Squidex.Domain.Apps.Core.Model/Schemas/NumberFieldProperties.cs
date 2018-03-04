// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(NumberField))]
    public sealed class NumberFieldProperties : FieldProperties
    {
        public ImmutableList<double> AllowedValues { get; set; }

        public double? MaxValue { get; set; }

        public double? MinValue { get; set; }

        public double? DefaultValue { get; set; }

        public bool InlineEditable { get; set; }

        public NumberFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new NumberField(id, name, partitioning, this);
        }
    }
}
