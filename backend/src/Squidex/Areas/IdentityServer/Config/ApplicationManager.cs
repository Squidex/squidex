// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace Squidex.Areas.IdentityServer.Config;

public sealed class ApplicationManager<T>(
    IOpenIddictApplicationCache<T> cache,
    ILogger<OpenIddictApplicationManager<T>> logger,
    IOptionsMonitor<OpenIddictCoreOptions> options,
    IOpenIddictApplicationStore<T> store)
    : OpenIddictApplicationManager<T>(cache, logger, options, store) where T : class
{
    protected override ValueTask<bool> ValidateClientSecretAsync(string secret, string comparand,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<bool>(string.Equals(secret, comparand, StringComparison.Ordinal));
    }
}
