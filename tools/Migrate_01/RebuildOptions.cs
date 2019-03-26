// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Migrate_01
{
    public sealed class RebuildOptions
    {
        public bool Apps { get; set; }

        public bool Assets { get; set; }

        public bool Contents { get; set; }

        public bool Rules { get; set; }

        public bool Schemas { get; set; }
    }
}
