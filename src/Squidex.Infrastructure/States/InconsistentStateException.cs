// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.States
{
    [Serializable]
    public class InconsistentStateException : Exception
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

        public InconsistentStateException(long currentVersion, long expectedVersion, Exception ex)
            : base(FormatMessage(currentVersion, expectedVersion), ex)
        {
            this.currentVersion = currentVersion;

            this.expectedVersion = expectedVersion;
        }

        protected InconsistentStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(long currentVersion, long expectedVersion)
        {
            return $"Requested version {expectedVersion}, but found {currentVersion}.";
        }
    }
}
