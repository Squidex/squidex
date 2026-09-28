// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class TextExtractionContext
{
    required public NamedId<DomainId> AppId { get; init; }

    required public NamedId<DomainId> SchemaId { get; init; }

    required public DomainId ContentId { get; init; }

    required public ContentData Data { get; init; }

    public Schema? Schema { get; init; }

    public ResolvedComponents Components { get; init; } = ResolvedComponents.Empty;
}
