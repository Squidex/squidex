// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("JsonField")]
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

        public override RootField CreateRootField(long id, string name, Partitioning partitioning)
        {
            return Fields.Json(id, name, partitioning, this);
        }

        public override NestedField CreateNestedField(long id, string name)
        {
            return Fields.Json(id, name, this);
        }
    }
}
