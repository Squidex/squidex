// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.EventSourcing
{
    [Serializable]
    public class WrongEventVersionException : Exception
    {
        private readonly long currentVersion;
        private readonly long expectedVersion;

        public long CurrentVersion
        {
            get { return currentVersion; }
        }

        public long ExpectedVersion
        {
            get { return expectedVersion; }
        }

        public WrongEventVersionException(long currentVersion, long expectedVersion)
            : base(FormatMessage(currentVersion, expectedVersion))
        {
            this.currentVersion = currentVersion;

            this.expectedVersion = expectedVersion;
        }

        protected WrongEventVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            currentVersion = info.GetInt64(nameof(currentVersion));

            expectedVersion = info.GetInt64(nameof(expectedVersion));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(currentVersion), currentVersion);
            info.AddValue(nameof(expectedVersion), expectedVersion);

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(long currentVersion, long expectedVersion)
        {
            return $"Requested version {expectedVersion}, but found {currentVersion}.";
        }
    }
}
