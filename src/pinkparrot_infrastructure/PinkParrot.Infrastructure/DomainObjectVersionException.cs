// ==========================================================================
//  DomainObjectVersionException.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure
{
    public class DomainObjectVersionException : DomainObjectException
    {
        private readonly int currentVersion;
        private readonly int expectedVersion;

        public int CurrentVersion
        {
            get
            {
                return currentVersion;
            }
        }

        public int ExpectedVersion
        {
            get
            {
                return expectedVersion;
            }
        }

        public DomainObjectVersionException(string id, Type type, int currentVersion, int expectedVersion)
            : base(FormatMessage(id, type, currentVersion, expectedVersion), id, type)
        {
            this.currentVersion = currentVersion;

            this.expectedVersion = expectedVersion;
        }

        private static string FormatMessage(string id, Type type, int currentVersion, int expectedVersion)
        {
            return $"Request version {expectedVersion} for object '{id}' (type {type}), but found {currentVersion}.";
        }
    }
}
