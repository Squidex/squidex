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
        public const long Unchanged = 13;

        public long Version { get; set; }

        public long Value { get; set; }

        public MyDomainState Apply(Envelope<IEvent> @event)
        {
            var value = @event.To<ValueChanged>().Payload.Value;

            if (value == Unchanged)
            {
                return this;
            }

            return new MyDomainState { Value = value };
        }
    }

    public sealed class ValueChanged : IEvent
    {
        public long Value { get; set; }
    }
}
