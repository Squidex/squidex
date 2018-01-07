// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class TagsField : Field<TagsFieldProperties>
    {
        public TagsField(long id, string name, Partitioning partitioning)
            : base(id, name, partitioning, new TagsFieldProperties())
        {
        }

        public TagsField(long id, string name, Partitioning partitioning, TagsFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
