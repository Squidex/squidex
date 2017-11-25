// ==========================================================================
//  JsonFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(JsonField))]
    public sealed class JsonFieldProperties : FieldProperties
    {
        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new JsonField(id, name, partitioning, this);
        }
    }
}
