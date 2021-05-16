// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace Squidex.Areas.IdentityServer.Config
{
    public class ApplicationManager<T> : OpenIddictApplicationManager<T> where T : class
    {
        private sealed class NoopCache<TApplication> : IOpenIddictApplicationCache<TApplication> where TApplication : class
        {
            private readonly IOpenIddictApplicationStoreResolver resolver;

            public NoopCache(IOpenIddictApplicationStoreResolver resolver)
            {
                this.resolver = resolver;
            }

            public ValueTask<TApplication?> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
            {
                var store = resolver.Get<TApplication>();

                return store.FindByClientIdAsync(identifier, cancellationToken);
            }

            public ValueTask<TApplication?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
            {
                var store = resolver.Get<TApplication>();

                return store.FindByIdAsync(identifier, cancellationToken);
            }

            public IAsyncEnumerable<TApplication> FindByPostLogoutRedirectUriAsync(string address, CancellationToken cancellationToken)
            {
                var store = resolver.Get<TApplication>();

                return store.FindByPostLogoutRedirectUriAsync(address, cancellationToken);
            }

            public IAsyncEnumerable<TApplication> FindByRedirectUriAsync(string address, CancellationToken cancellationToken)
            {
                var store = resolver.Get<TApplication>();

                return store.FindByRedirectUriAsync(address, cancellationToken);
            }

            public ValueTask AddAsync(TApplication application, CancellationToken cancellationToken)
            {
                return default;
            }

            public ValueTask RemoveAsync(TApplication application, CancellationToken cancellationToken)
            {
                return default;
            }
        }

        public ApplicationManager(
            ILogger<OpenIddictApplicationManager<T>> logger, IOptionsMonitor<OpenIddictCoreOptions> options,
            IOpenIddictApplicationStoreResolver resolver)
            : base(new NoopCache<T>(resolver), logger, options, resolver)
        {
        }

        protected override ValueTask<bool> ValidateClientSecretAsync(string secret, string comparand, CancellationToken cancellationToken = default)
        {
            if (string.Equals(secret, comparand))
            {
                return new ValueTask<bool>(true);
            }

            return base.ValidateClientSecretAsync(secret, comparand, cancellationToken);
        }
    }
}
