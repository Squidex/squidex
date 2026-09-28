// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public sealed class TextIndexBatch : IDisposable
{
    private readonly IDisposable handle;
    private readonly Dictionary<DomainId, TextIndexRebuildScope> scopes;

    internal TextIndexBatch(IDisposable handle, Dictionary<DomainId, TextIndexRebuildScope> scopes)
    {
        this.handle = handle;
        this.scopes = scopes;
    }

    public bool TrySkip(DomainId appId, DomainId schemaId, DomainId contentId)
    {
        if (!scopes.TryGetValue(appId, out var scope))
        {
            return false;
        }

        if (!scope.Includes(schemaId))
        {
            return false;
        }

        // The rebuild job has taken over the content, therefore it has to rebuild it again.
        scope.SkippedContents.Add(contentId);
        return true;
    }

    public void Dispose()
    {
        // The rebuild cannot hand back the app while the batch is processed.
        handle.Dispose();
    }
}
