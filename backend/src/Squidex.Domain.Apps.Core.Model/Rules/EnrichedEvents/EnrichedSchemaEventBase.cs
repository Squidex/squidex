// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public abstract class EnrichedSchemaEventBase : EnrichedUserEventBase
{
    [FieldDescription(nameof(FieldDescriptions.EntityVersion))]
    public NamedId<DomainId> SchemaId { get; set; }
}
