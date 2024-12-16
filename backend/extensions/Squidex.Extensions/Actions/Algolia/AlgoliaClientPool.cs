// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Algolia.Search.Clients;

namespace Squidex.Extensions.Actions.Algolia;

internal sealed class AlgoliaClientPool : ClientPool<(string AppId, string ApiKey, string IndexName), ISearchIndex>
{
    public AlgoliaClientPool()
        : base(CreateClient)
    {
    }

    private static ISearchIndex CreateClient((string AppId, string ApiKey, string IndexName) key)
    {
        var client = new SearchClient(key.AppId, key.ApiKey);

        return client.InitIndex(key.IndexName);
    }
}
