// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.States
{
    public sealed class DefaultStreamNameResolver : IStreamNameResolver
    {
        private const string Suffix = "DomainObject";

        public string GetStreamName(Type aggregateType, string id)
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
