// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using LettuceEncrypt;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Users
{
    public sealed class DefaultCertificateStore : ICertificateRepository, ICertificateSource
    {
        private readonly ISnapshotStore<State, Guid> store;

        [CollectionName("Identity_Certificates")]
        public sealed class State
        {
            public byte[] Certificate { get; set; }

            public State()
            {
            }

            public State(X509Certificate2 certificate)
            {
                Certificate = certificate.Export(X509ContentType.Pfx);
            }

            public X509Certificate2 ToCertificate()
            {
                return new X509Certificate2(Certificate);
            }
        }

        public DefaultCertificateStore(ISnapshotStore<State, Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public async Task<IEnumerable<X509Certificate2>> GetCertificatesAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<X509Certificate2>();

            await store.ReadAllAsync((state, _) =>
            {
                result.Add(state.ToCertificate());

                return Task.CompletedTask;
            }, cancellationToken);

            return result;
        }

        public Task SaveAsync(X509Certificate2 certificate, CancellationToken cancellationToken = default)
        {
            Guard.NotNull(certificate, nameof(certificate));

            var state = new State(certificate);

            return store.WriteAsync(Guid.NewGuid(), state, EtagVersion.Any, 0);
        }
    }
}
