// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class StreamMapper
    {
        private readonly Dictionary<string, long> streams = new Dictionary<string, long>(1000);
        private readonly RestoreContext context;
        private readonly DomainId brokenAppId;

        public StreamMapper(RestoreContext context)
        {
            Guard.NotNull(context, nameof(context));

            this.context = context;

            brokenAppId = DomainId.Combine(context.PreviousAppId, context.PreviousAppId);
        }

        public (string Stream, DomainId) Map(string stream)
        {
            Guard.NotNullOrEmpty(stream, nameof(stream));

            var typeIndex = stream.IndexOf("-", StringComparison.Ordinal);
            var typeName = stream.Substring(0, typeIndex);

            var id = DomainId.Create(stream[(typeIndex + 1)..]);

            if (id.Equals(context.PreviousAppId) || id.Equals(brokenAppId))
            {
                id = context.AppId;
            }
            else
            {
                var separator = DomainId.IdSeparator;

                var secondId = id.ToString().AsSpan();

                var indexOfSecondPart = secondId.IndexOf(separator, StringComparison.Ordinal);
                if (indexOfSecondPart > 0 && indexOfSecondPart < secondId.Length - separator.Length - 1)
                {
                    secondId = secondId[(indexOfSecondPart + separator.Length)..];
                }

                id = DomainId.Combine(context.AppId, DomainId.Create(secondId.ToString()));
            }

            stream = $"{typeName}-{id}";

            return (stream, id);
        }

        public long GetStreamOffset(string streamName)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (!streams.TryGetValue(streamName, out var offset))
            {
                offset = EtagVersion.Empty;
            }

            streams[streamName] = offset + 1;

            return offset;
        }
    }
}
