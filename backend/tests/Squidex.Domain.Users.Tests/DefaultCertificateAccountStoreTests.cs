// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using LettuceEncrypt.Accounts;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultCertificateAccountStoreTests
    {
        private readonly ISnapshotStore<DefaultCertificateAccountStore.State, Guid> store = A.Fake<ISnapshotStore<DefaultCertificateAccountStore.State, Guid>>();
        private readonly DefaultCertificateAccountStore sut;

        public DefaultCertificateAccountStoreTests()
        {
            sut = new DefaultCertificateAccountStore(store);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            var model = new AccountModel();

            A.CallTo(() => store.ReadAsync(default))
                .Returns((new DefaultCertificateAccountStore.State { Account = model }, 0));

            var result = await sut.GetAccountAsync(default);

            Assert.Same(model, result);
        }

        [Fact]
        public async Task Should_write_to_store()
        {
            var model = new AccountModel();

            await sut.SaveAccountAsync(model, default);

            A.CallTo(() => store.WriteAsync(A<Guid>._, A<DefaultCertificateAccountStore.State>._, A<long>._, 0))
                .MustHaveHappened();
        }
    }
}
