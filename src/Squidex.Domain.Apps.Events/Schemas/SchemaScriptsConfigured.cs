// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaScriptsConfigured))]
    public sealed class SchemaScriptsConfigured : SchemaEvent
    {
        public Dictionary<string, string> Scripts { get; set; }
    }
}
