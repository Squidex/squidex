// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class ConfigurePreviewUrls : SchemaCommand
    {
        public Dictionary<string, string> PreviewUrls { get; set; }
    }
}
