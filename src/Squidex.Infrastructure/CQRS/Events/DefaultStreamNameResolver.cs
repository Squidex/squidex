// ==========================================================================
//  DefaultStreamNameResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class DefaultStreamNameResolver : IStreamNameResolver
    {
        private const string Suffix = "DomainObject";

        public string GetStreamName(Type aggregateType, Guid id)
        {
            var typeName = char.ToLower(aggregateType.Name[0]) + aggregateType.Name.Substring(1);

            if (typeName.EndsWith(Suffix, StringComparison.Ordinal))
            {
                typeName = typeName.Substring(0, typeName.Length - Suffix.Length);
            }

            return $"{typeName}-{id}";
        }
    }
}
