// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Migrations
{
    public sealed class RebuildOptions
    {
        public bool Apps { get; set; }

        public bool Assets { get; set; }

        public bool AssetFiles { get; set; }

        public bool Contents { get; set; }

        public bool Indexes { get; set; }

        public bool Rules { get; set; }

        public bool Schemas { get; set; }

        public int BatchSize { get; set; } = 100;

        public int CalculateBatchSize()
        {
            return Math.Max(10, Math.Min(1000, BatchSize));
        }
    }
}
