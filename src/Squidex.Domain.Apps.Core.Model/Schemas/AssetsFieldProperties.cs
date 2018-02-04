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
        public bool MustBeImage { get; set; }

        public int? MinItems { get; set; }

        public int? MaxItems { get; set; }

        public int? MinWidth { get; set; }

        public int? MaxWidth { get; set; }

        public int? MinHeight { get; set; }

        public int? MaxHeight { get; set; }

        public int? MinSize { get; set; }

        public int? MaxSize { get; set; }

        public int? AspectWidth { get; set; }

        public int? AspectHeight { get; set; }

        public ImmutableList<string> AllowedExtensions { get; set; }

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
