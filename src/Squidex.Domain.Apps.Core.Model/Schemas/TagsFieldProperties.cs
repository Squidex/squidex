// ==========================================================================
//  TagsField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(TagsField))]
    public sealed class TagsFieldProperties : FieldProperties
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
            return new TagsField(id, name, partitioning, this);
        }
    }
}
