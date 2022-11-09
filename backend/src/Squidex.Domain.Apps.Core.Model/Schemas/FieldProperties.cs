// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public abstract record FieldProperties : NamedElementPropertiesBase
{
    public bool IsRequired { get; init; }

    public bool IsRequiredOnPublish { get; init; }

    public bool IsHalfWidth { get; init; }

    public string? Placeholder { get; init; }

    public string? EditorUrl { get; init; }

    public ReadonlyList<string>? Tags { get; init; }

    public abstract T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args);

    public abstract T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args);

    public abstract RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null);

    public abstract NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null);
}
