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

            return $"{aggregateType.TypeName(true, Suffixes)}-{id}";
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
