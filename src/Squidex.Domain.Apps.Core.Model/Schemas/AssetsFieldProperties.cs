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
    [TypeName(nameof(AssetsField))]
    public sealed class AssetsFieldProperties : FieldProperties
    {
        private bool mustBeImage;
        private int? minItems;
        private int? maxItems;
        private int? minWidth;
        private int? maxWidth;
        private int? minHeight;
        private int? maxHeight;
        private int? minSize;
        private int? maxSize;
        private int? aspectWidth;
        private int? aspectHeight;
        private ImmutableList<string> allowedExtensions;

        public bool MustBeImage
        {
            get
            {
                return mustBeImage;
            }
            set
            {
                ThrowIfFrozen();

                mustBeImage = value;
            }
        }

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

        public int? MinWidth
        {
            get
            {
                return minWidth;
            }
            set
            {
                ThrowIfFrozen();

                minWidth = value;
            }
        }

        public int? MaxWidth
        {
            get
            {
                return maxWidth;
            }
            set
            {
                ThrowIfFrozen();

                maxWidth = value;
            }
        }

        public int? MinHeight
        {
            get
            {
                return minHeight;
            }
            set
            {
                ThrowIfFrozen();

                minHeight = value;
            }
        }

        public int? MaxHeight
        {
            get
            {
                return maxHeight;
            }
            set
            {
                ThrowIfFrozen();

                maxHeight = value;
            }
        }

        public int? MinSize
        {
            get
            {
                return minSize;
            }
            set
            {
                ThrowIfFrozen();

                minSize = value;
            }
        }

        public int? MaxSize
        {
            get
            {
                return maxSize;
            }
            set
            {
                ThrowIfFrozen();

                maxSize = value;
            }
        }

        public int? AspectWidth
        {
            get
            {
                return aspectWidth;
            }
            set
            {
                ThrowIfFrozen();

                aspectWidth = value;
            }
        }

        public int? AspectHeight
        {
            get
            {
                return aspectHeight;
            }
            set
            {
                ThrowIfFrozen();

                aspectHeight = value;
            }
        }

        public ImmutableList<string> AllowedExtensions
        {
            get
            {
                return allowedExtensions;
            }
            set
            {
                ThrowIfFrozen();

                allowedExtensions = value;
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
