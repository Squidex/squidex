// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Users
{
    public sealed class DefaultCertificateStoreTests
    {
        private readonly ISnapshotStore<DefaultCertificateStore.State, Guid> store = A.Fake<ISnapshotStore<DefaultCertificateStore.State, Guid>>();
        private readonly DefaultCertificateStore sut;

        public DefaultCertificateStoreTests()
        {
            sut = new DefaultCertificateStore(store);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            A.CallTo(() => store.ReadAllAsync(A<Func<DefaultCertificateStore.State, long, Task>>._, A<CancellationToken>._))
                .Invokes((Func<DefaultCertificateStore.State, long, Task> callback, CancellationToken _) =>
                {
                    callback(new DefaultCertificateStore.State
                    {
                        Certificate = MakeCert().Export(X509ContentType.Pfx)
                    }, 0);

                    callback(new DefaultCertificateStore.State
                    {
                        Certificate = MakeCert().Export(X509ContentType.Pfx)
                    }, 0);
                });

            var xml = await sut.GetCertificatesAsync();

            Assert.Equal(2, xml.Count());
        }

        [Fact]
        public async Task Should_write_to_store()
        {
            var certificate = MakeCert();

            await sut.SaveAsync(certificate, default);

            A.CallTo(() => store.WriteAsync(A<Guid>._, A<DefaultCertificateStore.State>._, A<long>._, 0))
                .MustHaveHappened();
        }

        private static X509Certificate2 MakeCert()
        {
            var ecdsa = ECDsa.Create();

            var certificateRequest = new CertificateRequest("cn=foobar", ecdsa, HashAlgorithmName.SHA256);

            return certificateRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
        }
    }
}
