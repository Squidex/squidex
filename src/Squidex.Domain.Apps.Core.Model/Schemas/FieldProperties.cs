// ==========================================================================
//  FieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldProperties : NamedElementPropertiesBase
    {
        public bool IsRequired { get; set; }

        public bool IsListField { get; set; }

        public string Placeholder { get; set; }

        public abstract T Accept<T>(IFieldPropertiesVisitor<T> visitor);
    }
}