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
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.History
{
    public sealed class ParsedHistoryEvent
    {
        private readonly HistoryEvent item;
        private readonly Lazy<string?> message;

        public DomainId Id
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

        public string EventType
        {
            get { return item.EventType; }
        }

        public string? Message
        {
            get { return message.Value; }
        }

        public ParsedHistoryEvent(HistoryEvent item, IReadOnlyDictionary<string, string> texts)
        {
            this.item = item;

            message = new Lazy<string?>(() =>
            {
                if (texts.TryGetValue(item.EventType, out var translationKey))
                {
                    var result = T.Get(translationKey);

                    foreach (var (key, value) in item.Parameters)
                    {
                        result = result.Replace("[" + key + "]", value);
                    }

                    return result;
                }

                return null;
            });
        }
    }
}
