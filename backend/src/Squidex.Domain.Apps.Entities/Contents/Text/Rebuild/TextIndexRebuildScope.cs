// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

internal sealed class TextIndexRebuildScope(DomainId? schemaId)
{
    public HashSet<DomainId> SkippedContents { get; } = [];

    public bool Includes(DomainId eventSchemaId)
    {
        // Only skip the contents that are also rebuilt, so that the other schemas are indexed normally.
        return schemaId == null || schemaId == eventSchemaId;
    }
}
