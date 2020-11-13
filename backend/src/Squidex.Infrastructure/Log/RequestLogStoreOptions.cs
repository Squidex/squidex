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
        public int StoreRetentionInDays { get; set; } = 90;
    }
}
