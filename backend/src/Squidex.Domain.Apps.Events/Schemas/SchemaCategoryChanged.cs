// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas;

[EventType(nameof(SchemaCategoryChanged))]
public sealed class SchemaCategoryChanged : SchemaEvent
{
    public string? Name { get; set; }
}
