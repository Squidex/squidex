// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class SchemaTextExtractionStrategy(IEnumerable<ITextFieldStrategy> fieldStrategies, IEnumerable<ITextNormalizer> normalizers) : ITextExtractionStrategy
{
    private readonly ITextFieldStrategy[] fieldStrategies = fieldStrategies.OrderBy(x => x.Order).ToArray();
    private readonly ITextNormalizer[] normalizers = normalizers.OrderBy(x => x.Order).ToArray();

    public int Order => 0;

    public ExtractedTexts? Extract(TextExtractionContext context, DomainId contentId, ContentData data)
    {
        using var collector = new TextCollector();

        new ContentTextWalker(fieldStrategies, normalizers, context, collector).Walk(data);

        return collector.Build();
    }
}
