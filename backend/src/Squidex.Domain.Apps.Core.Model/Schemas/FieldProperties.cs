// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public abstract class FieldProperties : NamedElementPropertiesBase
    {
        public bool IsRequired { get; set; }

        public bool IsRequiredOnPublish { get; set; }

        public bool IsHalfWidth { get; set; }

        public string? Placeholder { get; set; }

        public string? EditorUrl { get; set; }

        public ReadOnlyCollection<string>? Tags { get; set; }

        public abstract T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args);

        public abstract T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args);

        public abstract RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null);

        public abstract NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null);
    }
}