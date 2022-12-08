// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas;

[EventType(nameof(SchemaFieldRulesConfigured))]
public sealed class SchemaFieldRulesConfigured : SchemaEvent
{
    public FieldRules? FieldRules { get; set; }
}
