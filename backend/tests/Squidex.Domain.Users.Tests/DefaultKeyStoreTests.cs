// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultKeyStoreTests
    {
        private readonly ISnapshotStore<DefaultKeyStore.State, Guid> store = A.Fake<ISnapshotStore<DefaultKeyStore.State, Guid>>();
        private readonly DefaultKeyStore sut;

        public DefaultKeyStoreTests()
        {
            sut = new DefaultKeyStore(store);
        }

        [Fact]
        public async Task Should_generate_signing_credentials_once()
        {
            A.CallTo(() => store.ReadAsync(A<Guid>._))
                .Returns((null!, 0));

            var credentials1 = await sut.GetSigningCredentialsAsync();
            var credentials2 = await sut.GetSigningCredentialsAsync();

            Assert.Same(credentials1, credentials2);

            A.CallTo(() => store.ReadAsync(A<Guid>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => store.WriteAsync(A<Guid>._, A<DefaultKeyStore.State>._, 0, 0))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_generate_validation_keys_once()
        {
            A.CallTo(() => store.ReadAsync(A<Guid>._))
                .Returns((null!, 0));

            var credentials1 = await sut.GetValidationKeysAsync();
            var credentials2 = await sut.GetValidationKeysAsync();

            Assert.Same(credentials1, credentials2);

            A.CallTo(() => store.ReadAsync(A<Guid>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => store.WriteAsync(A<Guid>._, A<DefaultKeyStore.State>._, 0, 0))
                .MustHaveHappenedOnceExactly();
        }
    }
}
