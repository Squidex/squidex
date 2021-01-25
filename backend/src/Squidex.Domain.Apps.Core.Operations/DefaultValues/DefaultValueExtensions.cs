// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.DefaultValues
{
    public static class DefaultValueExtensions
    {
        public static void GenerateDefaultValues(this ContentData data, Schema schema, PartitionResolver partitionResolver)
        {
            var enricher = new DefaultValueGenerator(schema, partitionResolver);

            enricher.Enrich(data);
        }
    }
}
