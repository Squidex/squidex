// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record SchemaProperties : NamedElementPropertiesBase
{
    public static readonly SchemaProperties Empty = new SchemaProperties();

    public ReadonlyList<string>? Tags { get; init; }

    public string? ContentsSidebarUrl { get; init; }

    public string? ContentSidebarUrl { get; init; }

    public string? ContentEditorUrl { get; init; }

    public bool ValidateOnPublish { get; init; }
}
