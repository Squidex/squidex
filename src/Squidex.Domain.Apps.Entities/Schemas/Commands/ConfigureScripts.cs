// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class ConfigureScripts : SchemaCommand
    {
        public string ScriptQuery { get; set; }

        public string ScriptCreate { get; set; }

        public string ScriptUpdate { get; set; }

        public string ScriptDelete { get; set; }

        public string ScriptChange { get; set; }
    }
}
