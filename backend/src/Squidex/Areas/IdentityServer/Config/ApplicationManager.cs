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

public sealed class ApplicationManager<T> : OpenIddictApplicationManager<T> where T : class
{
    public ApplicationManager(
        IOptionsMonitor<OpenIddictCoreOptions> options,
        IOpenIddictApplicationCache<T> cache,
        IOpenIddictApplicationStoreResolver resolver,
        ILogger<OpenIddictApplicationManager<T>> logger)
        : base(cache, logger, options, resolver)
    {
    }

    protected override ValueTask<bool> ValidateClientSecretAsync(string secret, string comparand,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<bool>(string.Equals(secret, comparand, StringComparison.Ordinal));
    }
}
