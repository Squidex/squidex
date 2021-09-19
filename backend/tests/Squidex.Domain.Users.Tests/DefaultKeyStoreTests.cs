// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using FakeItEasy;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultKeyStoreTests
    {
        private readonly ISnapshotStore<DefaultKeyStore.State> store = A.Fake<ISnapshotStore<DefaultKeyStore.State>>();
        private readonly DefaultKeyStore sut;

        public DefaultKeyStoreTests()
        {
            sut = new DefaultKeyStore(store);
        }

        [Fact]
        public void Should_configure_new_keys()
        {
            A.CallTo(() => store.ReadAsync(A<DomainId>._, default))
                .Returns((null!, true, 0));

            var options = new OpenIddictServerOptions();

            sut.Configure(options);

            Assert.NotEmpty(options.SigningCredentials);
            Assert.NotEmpty(options.EncryptionCredentials);

            A.CallTo(() => store.WriteAsync(A<DomainId>._, A<DefaultKeyStore.State>._, 0, 0, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Should_configure_existing_keys()
        {
            A.CallTo(() => store.ReadAsync(A<DomainId>._, default))
                .Returns((ExistingKey(), true, 0));

            var options = new OpenIddictServerOptions();

            sut.Configure(options);

            Assert.NotEmpty(options.SigningCredentials);
            Assert.NotEmpty(options.EncryptionCredentials);

            A.CallTo(() => store.WriteAsync(A<DomainId>._, A<DefaultKeyStore.State>._, 0, 0, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_configure_existing_keys_when_initial_setup_failed()
        {
            A.CallTo(() => store.ReadAsync(A<DomainId>._, default))
                .Returns((null!, true, 0)).Once()
                .Then
                .Returns((ExistingKey(), true, 0));

            A.CallTo(() => store.WriteAsync(A<DomainId>._, A<DefaultKeyStore.State>._, 0, 0, default))
                .Throws(new InconsistentStateException(0, 0));

            var options = new OpenIddictServerOptions();

            sut.Configure(options);

            Assert.NotEmpty(options.SigningCredentials);
            Assert.NotEmpty(options.EncryptionCredentials);

            A.CallTo(() => store.WriteAsync(A<DomainId>._, A<DefaultKeyStore.State>._, 0, 0, default))
                .MustHaveHappened();
        }

        private static DefaultKeyStore.State ExistingKey()
        {
            var key = new RsaSecurityKey(RSA.Create(2048))
            {
                KeyId = CryptoRandom.CreateUniqueId(16)
            };

            return new DefaultKeyStore.State
            {
                Parameters = key.Rsa.ExportParameters(includePrivateParameters: true)
            };
        }
    }
}
