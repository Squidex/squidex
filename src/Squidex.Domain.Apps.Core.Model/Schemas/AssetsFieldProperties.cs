// ==========================================================================
//  AssetsFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(AssetsField))]
    public sealed class AssetsFieldProperties : FieldProperties
    {
        private int? minItems;
        private int? maxItems;

        public int? MinItems
        {
            get
            {
                return minItems;
            }
            set
            {
                ThrowIfFrozen();

                minItems = value;
            }
        }

        public int? MaxItems
        {
            get
            {
                return maxItems;
            }
            set
            {
                ThrowIfFrozen();

                maxItems = value;
            }
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new AssetsField(id, name, partitioning, this);
        }
    }
}
