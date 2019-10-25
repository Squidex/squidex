// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.TestHelpers
{
    public sealed class MyDomainState : IDomainState<MyDomainState>
    {
        public long Version { get; set; }

        public int Value { get; set; }

        public MyDomainState Apply(Envelope<IEvent> @event)
        {
            return new MyDomainState { Value = ((ValueChanged)@event.Payload).Value };
        }
    }

    public sealed class ValueChanged : IEvent
    {
        public int Value { get; set; }
    }
}
