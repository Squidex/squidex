// ==========================================================================
//  DefaultNameResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public sealed class DefaultNameResolver : IStreamNameResolver
    {
        private const string Suffix = "DomainObject";
        private readonly string prefix;
        
        public DefaultNameResolver(string prefix)
        {
            Guard.NotNullOrEmpty(prefix, nameof(prefix));

            this.prefix = prefix;
        }

        public string GetStreamName(Type aggregateType, Guid id)
        {
            var typeName = char.ToLower(aggregateType.Name[0]) + aggregateType.Name.Substring(1);

            if (typeName.EndsWith(Suffix))
            {
                typeName = typeName.Substring(0, typeName.Length - Suffix.Length);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", prefix, typeName, id);
        }
    }
}
