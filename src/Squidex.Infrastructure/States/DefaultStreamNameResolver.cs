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
        private static readonly string[] Suffixes = { "Grain", "DomainObject" };

        public string GetStreamName(Type aggregateType, string id)
        {
            var typeName = char.ToLower(aggregateType.Name[0]) + aggregateType.Name.Substring(1);

            foreach (var suffix in Suffixes)
            {
                if (typeName.EndsWith(suffix, StringComparison.Ordinal))
                {
                    typeName = typeName.Substring(0, typeName.Length - suffix.Length);

                    break;
                }
            }

            return $"{typeName}-{id}";
        }
    }
}
