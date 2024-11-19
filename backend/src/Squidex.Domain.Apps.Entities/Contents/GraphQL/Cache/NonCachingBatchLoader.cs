// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.DataLoader;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Cache;

internal class NonCachingBatchLoader<TKey, T>(Func<IEnumerable<TKey>, CancellationToken, Task<IDictionary<TKey, T>>> queryDelegate, int maxBatchSize = int.MaxValue) : DataLoaderBase<TKey, T>(false, maxBatchSize) where TKey : notnull where T : class
{
    protected override async Task FetchAsync(IEnumerable<DataLoaderPair<TKey, T>> list,
        CancellationToken cancellationToken)
    {
        var dictionary = await queryDelegate(list.Select(x => x.Key), cancellationToken).ConfigureAwait(false);

        foreach (var item in list)
        {
            dictionary.TryGetValue(item.Key, out var value);

            item.SetResult(value!);
        }
    }
}
