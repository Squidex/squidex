// ==========================================================================
//  HistoryEventToStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Read.History
{
    public class HistoryEventToStore
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        public string Channel { get; }

        public string Message { get; }

        public IReadOnlyDictionary<string, string> Parameters
        {
            get { return parameters; }
        }

        public static HistoryEventToStore Create(IEvent @event, string channel)
        {
            Guard.NotNull(@event, nameof(@event));

            return new HistoryEventToStore(channel, TypeNameRegistry.GetName(@event.GetType()));
        }

        public HistoryEventToStore(string channel, string message)
        {
            Guard.NotNullOrEmpty(channel, nameof(channel));
            Guard.NotNullOrEmpty(message, nameof(message));

            Channel = channel;
            Message = message;
        }

        public HistoryEventToStore AddParameter(string key, string value)
        {
            parameters[key] = value;

            return this;
        }
    }
}
