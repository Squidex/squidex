// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public record struct SubscriptionQuery
    {
        public string? Position { get; set; }

        public string? StreamFilter { get; set; }

        public Dictionary<string, string>? Context { get; set; }
    }
}
