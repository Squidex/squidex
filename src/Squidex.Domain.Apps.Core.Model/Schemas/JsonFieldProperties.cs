// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class JsonFieldProperties : FieldProperties
    {
        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<JsonFieldProperties>)field);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings settings = null)
        {
            return Fields.Json(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings settings = null)
        {
            return Fields.Json(id, name, this, settings);
        }
    }
}
