// ==========================================================================
//  ScriptsConfigured.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("ScriptsConfiguredEvent")]
    public sealed class ScriptsConfigured : SchemaEvent
    {
        public string ScriptQuery { get; set; }

        public string ScriptCreate { get; set; }

        public string ScriptUpdate { get; set; }

        public string ScriptDelete { get; set; }

        public string ScriptPublish { get; set; }

        public string ScriptUnpublish { get; set; }
    }
}
