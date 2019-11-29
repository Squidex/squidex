﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldProperties : NamedElementPropertiesBase
    {
        public bool IsRequired { get; set; }

        public string? Placeholder { get; set; }

        public string? EditorUrl { get; set; }

        public ReadOnlyCollection<string> Tags { get; set; }

        public abstract T Accept<T>(IFieldPropertiesVisitor<T> visitor);

        public abstract T Accept<T>(IFieldVisitor<T> visitor, IField field);

        public abstract RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null);

        public abstract NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null);
    }
}