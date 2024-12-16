// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenSearch.Net;

namespace Squidex.Extensions.Actions.OpenSearch;

internal sealed class OpenSearchClientPool : ClientPool<(Uri Host, string? Username, string? Password), OpenSearchLowLevelClient>
{
    public OpenSearchClientPool()
        : base(CreateClient)
    {
    }

    private static OpenSearchLowLevelClient CreateClient((Uri Host, string? Username, string? Password) key)
    {
        var config = new ConnectionConfiguration(key.Host);

        if (!string.IsNullOrEmpty(key.Username) && !string.IsNullOrWhiteSpace(key.Password))
        {
            config = config.BasicAuthentication(key.Username, key.Password);
        }

        return new OpenSearchLowLevelClient(config);
    }
}
