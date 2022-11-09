// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Extensions.Assets.Azure;

public sealed class AzureMetadataSourceOptions
{
    public string Endpoint { get; set; }

    public string ApiKey { get; set; }

    public bool IsConfigured()
    {
        return
            !string.IsNullOrWhiteSpace(Endpoint) &&
            !string.IsNullOrWhiteSpace(ApiKey);
    }
}
