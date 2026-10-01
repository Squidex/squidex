// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public sealed class TextIndexBatch(IDisposable handle, Dictionary<DomainId, long> skippedEvents) : IDisposable
{
    public bool TrySkip(DomainId appId)
    {
        if (!skippedEvents.TryGetValue(appId, out var count))
        {
            return false;
        }

        // The rebuild reads the event from the event store, but it must know that it has to catch up.
        skippedEvents[appId] = count + 1;
        return true;
    }

    public void Dispose()
    {
        // The rebuild cannot hand back the app while the batch is processed.
        handle.Dispose();
    }
}
