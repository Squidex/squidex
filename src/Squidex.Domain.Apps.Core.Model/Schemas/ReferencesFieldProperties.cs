// ==========================================================================
//  ReferencesFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
    }
}
