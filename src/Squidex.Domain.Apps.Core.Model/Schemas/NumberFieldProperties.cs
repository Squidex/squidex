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
    [TypeName("NumberField")]
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

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<NumberFieldProperties>)field);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning)
        {
            return Fields.Number(id, name, partitioning, this);
        }

        public override NestedField CreateNestedField(long id, string name)
        {
            return Fields.Number(id, name, this);
        }
    }
}
