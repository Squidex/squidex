// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.TestHelpers
{
    public sealed record MyDomainState : IDomainState<MyDomainState>
    {
        public const long Unchanged = 13;

        public bool IsDeleted { get; set; }

        public long Version { get; set; }

        public long Value { get; set; }

        public MyDomainState Apply(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case ValueChanged valueChanged when valueChanged.Value != Unchanged:
                    return this with { Value = valueChanged.Value };
                case Deleted when !IsDeleted:
                    return this with { IsDeleted = true };
            }

            return this;
        }
    }

    public sealed class MultipleByTwiceEvent : IEvent, IMigratedStateEvent<MyDomainState>
    {
        public IEvent Migrate(MyDomainState state)
        {
            return new ValueChanged
            {
                Value = state.Value * 2
            };
        }
    }

    public sealed class ValueChanged : IEvent
    {
        public long Value { get; set; }
    }

    public sealed class Deleted : IEvent
    {
    }
}
