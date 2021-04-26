// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.States
{
    [Serializable]
    public class InconsistentStateException : Exception
    {
        public long CurrentVersion { get; }

        public long ExpectedVersion { get; }

        public InconsistentStateException(long currentVersion, long expectedVersion, Exception? inner = null)
            : base(FormatMessage(currentVersion, expectedVersion), inner)
        {
            CurrentVersion = currentVersion;

            ExpectedVersion = expectedVersion;
        }

        protected InconsistentStateException(SerializationInfo info, StreamingContext context)
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

        private static string FormatMessage(long currentVersion, long expectedVersion)
        {
            return $"Requested version {expectedVersion}, but found {currentVersion}.";
        }
    }
}
