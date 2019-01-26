// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class ConfigureScripts : SchemaCommand
    {
        public Dictionary<string, string> Scripts { get; set; }
    }
}
