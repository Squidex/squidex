﻿// ==========================================================================
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

        public InconsistentStateException(long currentVersion, long expectedVersion, Exception inner = null)
            : base(FormatMessage(currentVersion, expectedVersion), inner)
        {
            this.currentVersion = currentVersion;

            this.expectedVersion = expectedVersion;
        }

        protected InconsistentStateException(SerializationInfo info, StreamingContext context)
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
