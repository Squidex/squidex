// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public abstract class EnrichedSchemaEventBase : EnrichedUserEventBase
    {
        public NamedId<Guid> SchemaId { get; set; }
    }
}
