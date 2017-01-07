// ==========================================================================
//  WrongEventVersionException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class WrongEventVersionException : Exception
    {
        private readonly int currentVersion;
        private readonly int expectedVersion;

        public int CurrentVersion
        {
            get { return currentVersion; }
        }

        public int ExpectedVersion
        {
            get { return expectedVersion; }
        }

        public WrongEventVersionException(int currentVersion, int expectedVersion)
            : base(FormatMessage(currentVersion, expectedVersion))
        {
            this.currentVersion = currentVersion;

            this.expectedVersion = expectedVersion;
        }

        private static string FormatMessage(int currentVersion, int expectedVersion)
        {
            return $"Requested version {expectedVersion}, but found {currentVersion}.";
        }
    }
}
