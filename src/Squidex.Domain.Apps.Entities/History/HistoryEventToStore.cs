// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class HistoryEventToStore
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        public string Channel { get; }

        public string Message { get; }

        public IReadOnlyDictionary<string, string> Parameters
        {
            get { return parameters; }
        }

        public HistoryEventToStore(string channel, string message)
        {
            Guard.NotNullOrEmpty(channel, nameof(channel));
            Guard.NotNullOrEmpty(message, nameof(message));

            Channel = channel;

            Message = message;
        }

        public HistoryEventToStore AddParameter(string key, object value)
        {
            parameters[key] = value.ToString();

            return this;
        }
    }
}
