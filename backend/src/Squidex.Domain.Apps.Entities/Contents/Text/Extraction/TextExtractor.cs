// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class TextExtractor(IEnumerable<ITextExtractionStrategy> strategies)
{
    private readonly ITextExtractionStrategy[] strategies = strategies.OrderBy(x => x.Order).ToArray();

    public ExtractedTexts? Extract(TextExtractionContext context, DomainId contentId, ContentData data)
    {
        // The first strategy that returns a result wins, e.g. the index script before the schema.
        foreach (var strategy in strategies)
        {
            var result = strategy.Extract(context, contentId, data);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
