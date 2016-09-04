// ==========================================================================
//  DefaultNameResolver.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public sealed class DefaultNameResolver : IStreamNameResolver
    {
        private readonly string prefix;

        public DefaultNameResolver()
            : this(string.Empty)
        {
        }

        public DefaultNameResolver(string prefix)
        {
            this.prefix = prefix;
        }

        public string GetStreamName(Type aggregateType, Guid id)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", prefix, char.ToLower(aggregateType.Name[0]) + aggregateType.Name.Substring(1), id);
        }
    }
}
