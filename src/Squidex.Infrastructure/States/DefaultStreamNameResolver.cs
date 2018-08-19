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
        private static readonly string[] Suffixes = { "Grain", "DomainObject", "State" };

        public string GetStreamName(Type aggregateType, string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(aggregateType, nameof(aggregateType));

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

        public string WithNewId(string streamName, Func<string, string> idGenerator)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));
            Guard.NotNull(idGenerator, nameof(idGenerator));

            var positionOfDash = streamName.IndexOf('-');

            if (positionOfDash >= 0)
            {
                var newId = idGenerator(streamName.Substring(positionOfDash + 1));

                if (!string.IsNullOrWhiteSpace(newId))
                {
                    streamName = $"{streamName.Substring(0, positionOfDash)}-{newId}";
                }
            }

            return streamName;
        }
    }
}
