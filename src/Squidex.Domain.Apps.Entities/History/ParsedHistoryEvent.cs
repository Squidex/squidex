// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class ParsedHistoryEvent
    {
        private readonly HistoryEvent item;
        private readonly Lazy<string> message;

        public Guid Id
        {
            get { return item.Id; }
        }

        public Instant Created
        {
            get { return item.Created; }
        }

        public RefToken Actor
        {
            get { return item.Actor; }
        }

        public long Version
        {
            get { return item.Version; }
        }

        public string Channel
        {
            get { return item.Channel; }
        }

        public string Message
        {
            get { return message.Value; }
        }

        public ParsedHistoryEvent(HistoryEvent item, IReadOnlyDictionary<string, string> texts)
        {
            this.item = item;

            message = new Lazy<string>(() =>
            {
                if (texts.TryGetValue(item.Message, out var result))
                {
                    foreach (var kvp in item.Parameters)
                    {
                        result = result.Replace("[" + kvp.Key + "]", kvp.Value);
                    }

                    return result;
                }

                return null;
            });
        }
    }
}
