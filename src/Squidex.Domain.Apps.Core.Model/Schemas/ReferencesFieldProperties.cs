// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(ReferencesField))]
    public sealed class ReferencesFieldProperties : FieldProperties
    {
        public int? MinItems { get; set; }

        public int? MaxItems { get; set; }

        public Guid SchemaId { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new ReferencesField(id, name, partitioning, this);
        }
    }
}
