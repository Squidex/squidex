// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldProperties : NamedElementPropertiesBase
    {
        public bool IsRequired { get; set; }

        public bool IsListField { get; set; }

        public string Placeholder { get; set; }

        public abstract T Accept<T>(IFieldPropertiesVisitor<T> visitor);

        public abstract Field CreateField(long id, string name, Partitioning partitioning);
    }
}