// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public interface IContentExportWriter : IAsyncDisposable
{
    Task StartAsync(
        CancellationToken ct);

    Task WriteAsync(EnrichedContent content,
        CancellationToken ct);

    Task CompleteAsync(
        CancellationToken ct);
}
