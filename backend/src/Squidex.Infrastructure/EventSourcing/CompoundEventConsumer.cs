﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class CompoundEventConsumer : IEventConsumer
    {
        private readonly IEventConsumer[] inners;

        public string Name { get; }

        public string EventsFilter { get; }

        public CompoundEventConsumer(IEventConsumer first, params IEventConsumer[] inners)
            : this(first?.Name!, first!, inners)
        {
        }

        public CompoundEventConsumer(IEventConsumer[] inners)
        {
            Guard.NotNull(inners);
            Guard.NotEmpty(inners);

            this.inners = inners;

            Name = inners.First().Name;

            var innerFilters =
                this.inners.Where(x => !string.IsNullOrWhiteSpace(x.EventsFilter))
                    .Select(x => $"({x.EventsFilter})");

            EventsFilter = string.Join("|", innerFilters);
        }

        public CompoundEventConsumer(string name, IEventConsumer first, params IEventConsumer[] inners)
        {
            Guard.NotNull(first);
            Guard.NotNull(inners);
            Guard.NotNullOrEmpty(name);

            this.inners = new[] { first }.Union(inners).ToArray();

            Name = name;

            var innerFilters =
                this.inners.Where(x => !string.IsNullOrWhiteSpace(x.EventsFilter))
                    .Select(x => $"({x.EventsFilter})");

            EventsFilter = string.Join("|", innerFilters);
        }

        public bool Handles(StoredEvent @event)
        {
            return inners.Any(x => x.Handles(@event));
        }

        public Task ClearAsync()
        {
            return Task.WhenAll(inners.Select(i => i.ClearAsync()));
        }

        public async Task On(Envelope<IEvent> @event)
        {
            foreach (var inner in inners)
            {
                await inner.On(@event);
            }
        }
    }
}
