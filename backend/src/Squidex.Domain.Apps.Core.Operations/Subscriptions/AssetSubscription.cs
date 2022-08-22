// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions
{
    public sealed class AssetSubscription : ISubscription
    {
        public DomainId AppId { get; set; }

        public EnrichedAssetEventType? Type { get; set; }

        public ValueTask<bool> ShouldHandle(object message)
        {
            return new ValueTask<bool>(ShouldHandleCore(message));
        }

        private bool ShouldHandleCore(object message)
        {
            if (message is not EnrichedAssetEvent @event)
            {
                return false;
            }

            return Type == null || @event.Type == Type.Value;
        }
    }
}
