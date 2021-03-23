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
    public sealed class RestoreContext : BackupContextBase
    {
        private string? appStream;

        public IBackupReader Reader { get; }

        public DomainId PreviousAppId { get; set; }

        public Dictionary<string, long> Streams { get; } = new Dictionary<string, long>(1000);

        public string AppStream { get; }

        public RestoreContext(DomainId appId, IUserMapping userMapping, IBackupReader reader, DomainId previousAppId)
            : base(appId, userMapping)
        {
            Guard.NotNull(reader, nameof(reader));

            Reader = reader;

            PreviousAppId = previousAppId;
        }

        public string GetStreamName(string streamName)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (streamName.StartsWith("app-", StringComparison.OrdinalIgnoreCase))
            {
                return appStream ??= $"app-{AppId}";
            }

            return streamName.Replace(PreviousAppId.ToString(), AppId.ToString());
        }

        public long GetStreamOffset(string streamName)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (!Streams.TryGetValue(streamName, out var offset))
            {
                offset = EtagVersion.Empty;
            }

            Streams[streamName] = offset + 1;

            return offset;
        }
    }
}
