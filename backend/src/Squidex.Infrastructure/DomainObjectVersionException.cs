// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class DomainObjectVersionException : DomainObjectException
    {
        public long CurrentVersion { get; }

        public long ExpectedVersion { get; }

        public DomainObjectVersionException(string id, Type type, long currentVersion, long expectedVersion)
            : base(FormatMessage(id, type, currentVersion, expectedVersion), id, type)
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

        private static string FormatMessage(string id, Type type, long currentVersion, long expectedVersion)
        {
            return $"Requested version {expectedVersion} for object '{id}' (type {type}), but found {currentVersion}.";
        }
    }
}
