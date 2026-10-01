// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public interface ITextExtractionStrategy
{
    int Order { get; }

    ExtractedTexts? Extract(TextExtractionContext context, DomainId contentId, ContentData data);
}
