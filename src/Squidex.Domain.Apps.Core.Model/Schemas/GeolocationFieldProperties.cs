// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("GeolocationField")]
    public sealed class GeolocationFieldProperties : FieldProperties
    {
        public GeolocationFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<GeolocationFieldProperties>)field);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new Field<GeolocationFieldProperties>(id, name, partitioning, this);
        }
    }
}
