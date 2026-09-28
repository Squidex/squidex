// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class TextExtractor(IEnumerable<ITextExtractionStrategy> strategies)
{
    private readonly ITextExtractionStrategy[] strategies = strategies.OrderBy(x => x.Order).ToArray();

    public async ValueTask<Dictionary<string, string>?> ExtractAsync(TextExtractionContext context,
        CancellationToken ct = default)
    {
        // The first strategy that returns a result wins, e.g. the index script before the schema.
        foreach (var strategy in strategies)
        {
            var result = await strategy.ExtractAsync(context, ct);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
