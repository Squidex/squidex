// ==========================================================================
//  DomainObjectVersionException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
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

        private static string FormatMessage(string id, Type type, long currentVersion, long expectedVersion)
        {
            return $"Requested version {expectedVersion} for object '{id}' (type {type}), but found {currentVersion}.";
        }
    }
}
