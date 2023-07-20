// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.DataLoader;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Cache;

public sealed class EmptyDataLoaderResult<T> : IDataLoaderResult<T[]>
{
    public Task<T[]> GetResultAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<T>());
    }

    Task<object?> IDataLoaderResult.GetResultAsync(
        CancellationToken cancellationToken)
    {
        return Task.FromResult<object?>(Array.Empty<T>());
    }
}
