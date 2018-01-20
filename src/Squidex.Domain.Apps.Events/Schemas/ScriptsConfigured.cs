// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(ScriptsConfigured))]
    public sealed class ScriptsConfigured : SchemaEvent
    {
        public string ScriptQuery { get; set; }

        public string ScriptCreate { get; set; }

        public string ScriptUpdate { get; set; }

        public string ScriptDelete { get; set; }

        public string ScriptChange { get; set; }
    }
}
