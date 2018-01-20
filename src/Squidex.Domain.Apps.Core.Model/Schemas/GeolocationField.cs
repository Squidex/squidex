// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class GeolocationField : Field<GeolocationFieldProperties>
    {
        public GeolocationField(long id, string name, Partitioning partitioning)
            : base(id, name, partitioning, new GeolocationFieldProperties())
        {
        }

        public GeolocationField(long id, string name, Partitioning partitioning, GeolocationFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
