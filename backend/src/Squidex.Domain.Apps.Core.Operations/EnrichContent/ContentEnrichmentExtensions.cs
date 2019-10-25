// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.EnrichContent
{
    public static class ContentEnrichmentExtensions
    {
        public static void Enrich(this NamedContentData data, Schema schema, PartitionResolver partitionResolver)
        {
            var enricher = new ContentEnricher(schema, partitionResolver);

            enricher.Enrich(data);
        }
    }
}
