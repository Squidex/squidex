// ==========================================================================
//  AppPattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public class AppPattern
    {
        private readonly Guid id;
        private readonly string name;
        private readonly string pattern;
        private readonly string defaultMessage;

        public Guid Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Pattern
        {
            get { return pattern; }
        }

        public string DefaultMessage
        {
            get { return defaultMessage; }
        }

        public AppPattern(Guid id, string name, string pattern, string defaultMessage)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(pattern, nameof(pattern));

            this.id = id;
            this.name = name;
            this.pattern = pattern;
            this.defaultMessage = defaultMessage;
        }

        [Pure]
        public AppPattern Update(string name, string pattern, string defaultMessage)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNullOrEmpty(pattern, nameof(pattern));

            return new AppPattern(this.id, name, pattern, defaultMessage);
        }
    }
}
