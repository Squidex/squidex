// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Log
{
    public sealed class RequestLogStoreOptions
    {
        public bool StoreEnabled { get; set; }

        public int StoreRetentionInDays { get; set; } = 90;

        public int BatchSize { get; set; } = 1000;

        public int WriteIntervall { get; set; } = 1000;
    }
}
