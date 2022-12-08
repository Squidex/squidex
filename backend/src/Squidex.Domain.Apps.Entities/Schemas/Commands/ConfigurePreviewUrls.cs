// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public sealed class ConfigurePreviewUrls : SchemaCommand
{
    public ReadonlyDictionary<string, string>? PreviewUrls { get; set; }
}
