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

        public DomainObjectVersionException(string id, Type type, long currentVersion, long expectedVersion)
            : base(FormatMessage(id, type, currentVersion, expectedVersion), id, type)
        {
            this.currentVersion = currentVersion;

            this.expectedVersion = expectedVersion;
        }

        protected DomainObjectVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string id, Type type, long currentVersion, long expectedVersion)
        {
            return $"Requested version {expectedVersion} for object '{id}' (type {type}), but found {currentVersion}.";
        }
    }
}
