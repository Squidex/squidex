// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup.State
{
    [DataContract]
    public sealed class RestoreStateJob : IRestoreJob
    {
        [DataMember]
        public string AppName { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid AppId { get; set; }

        [DataMember]
        public RefToken Actor { get; set; }

        [DataMember]
        public Uri Url { get; set; }

        [DataMember]
        public string NewAppName { get; set; }

        [DataMember]
        public Instant Started { get; set; }

        [DataMember]
        public Instant? Stopped { get; set; }

        [DataMember]
        public List<string> Log { get; set; } = new List<string>();

        [DataMember]
        public JobStatus Status { get; set; }
    }
}
