// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions
{
    public sealed class SubscriptionPublisher : IEventConsumer
    {
        private readonly ISubscriptionService subscriptions;

        public string Name { get; } = "Subscriptions";

        public SubscriptionPublisher(ISubscriptionService subscriptions)
        {
            this.subscriptions = subscriptions;
        }

        public Task On(Envelope<IEvent> @event)
        {
            return Task.CompletedTask;
        }
    }
}
