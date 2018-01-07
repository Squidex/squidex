// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure;

#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter

namespace Squidex.Domain.Apps.Entities.MongoDb.History
{
    internal sealed class ParsedHistoryEvent : IHistoryEventEntity
    {
        private readonly MongoHistoryEventEntity inner;
        private readonly Lazy<string> message;

        public Guid Id
        {
            get { return inner.Id; }
            set { }
        }

        public Instant Created
        {
            get { return inner.Created; }
            set { }
        }

        public Instant LastModified
        {
            get { return inner.LastModified; }
            set { }
        }

        public RefToken Actor
        {
            get { return inner.Actor; }
        }

        public Guid EventId
        {
            get { return inner.Id; }
        }

        public long Version
        {
            get { return inner.Version; }
        }

        public string Channel
        {
            get { return inner.Channel; }
        }

        public string Message
        {
            get { return message.Value; }
        }

        public ParsedHistoryEvent(MongoHistoryEventEntity inner, IReadOnlyDictionary<string, string> texts)
        {
            this.inner = inner;

            message = new Lazy<string>(() =>
            {
                var result = texts[inner.Message];

                foreach (var kvp in inner.Parameters)
                {
                    result = result.Replace("[" + kvp.Key + "]", kvp.Value);
                }

                return result;
            });
        }
    }
}
