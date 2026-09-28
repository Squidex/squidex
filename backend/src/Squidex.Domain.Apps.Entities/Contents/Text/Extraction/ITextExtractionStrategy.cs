// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public interface ITextExtractionStrategy
{
    int Order { get; }

    ValueTask<Dictionary<string, string>?> ExtractAsync(TextExtractionContext context,
        CancellationToken ct);
}
