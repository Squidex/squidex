// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas;

[EventType(nameof(SchemaPreviewUrlsConfigured))]
public sealed class SchemaPreviewUrlsConfigured : SchemaEvent
{
    public ReadonlyDictionary<string, string>? PreviewUrls { get; set; }
}
