// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Elasticsearch.Net;

namespace Squidex.Extensions.Actions.ElasticSearch;

internal sealed class ElasticSearchClientPool : ClientPool<(Uri Host, string? Username, string? Password), ElasticLowLevelClient>
{
    public ElasticSearchClientPool()
        : base(CreateClient)
    {
    }

    private static ElasticLowLevelClient CreateClient((Uri Host, string? Username, string? Password) key)
    {
        var config = new ConnectionConfiguration(key.Host);

        if (!string.IsNullOrEmpty(key.Username) && !string.IsNullOrWhiteSpace(key.Password))
        {
            config = config.BasicAuthentication(key.Username, key.Password);
        }

        return new ElasticLowLevelClient(config);
    }
}
