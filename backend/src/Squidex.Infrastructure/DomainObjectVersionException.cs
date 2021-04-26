// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class DomainObjectVersionException : DomainObjectException
    {
        private const string ValidationError = "OBJECT_VERSION_CONFLICT";

        public long CurrentVersion { get; }

        public long ExpectedVersion { get; }

        public DomainObjectVersionException(string id, long currentVersion, long expectedVersion, Exception? inner = null)
            : base(FormatMessage(id, currentVersion, expectedVersion), id, ValidationError, inner)
        {
            CurrentVersion = currentVersion;

            ExpectedVersion = expectedVersion;
        }

        protected DomainObjectVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CurrentVersion = info.GetInt64(nameof(CurrentVersion));

            ExpectedVersion = info.GetInt64(nameof(ExpectedVersion));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(CurrentVersion), CurrentVersion);
            info.AddValue(nameof(ExpectedVersion), ExpectedVersion);

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(string id, long currentVersion, long expectedVersion)
        {
            return T.Get("exceptions.domainObjectVersion", new { id, currentVersion, expectedVersion });
        }
    }
}
