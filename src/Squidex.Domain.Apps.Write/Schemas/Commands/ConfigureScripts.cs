// ==========================================================================
//  ConfigureScripts.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class ConfigureScripts : SchemaAggregateCommand
    {
        public string ScriptQuery { get; set; }

        public string ScriptCreate { get; set; }

        public string ScriptUpdate { get; set; }

        public string ScriptDelete { get; set; }

        public string ScriptPublish { get; set; }

        public string ScriptUnpublish { get; set; }
    }
}
