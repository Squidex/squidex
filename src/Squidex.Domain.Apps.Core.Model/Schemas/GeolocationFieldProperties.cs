// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(GeolocationField))]
    public sealed class GeolocationFieldProperties : FieldProperties
    {
        private GeolocationFieldEditor editor;

        public GeolocationFieldEditor Editor
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

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new GeolocationField(id, name, partitioning, this);
        }
    }
}
