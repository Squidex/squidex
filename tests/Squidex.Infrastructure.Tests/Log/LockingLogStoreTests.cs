// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Infrastructure.Log
{
    public class LockingLogStoreTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ILockGrain lockGrain = A.Fake<ILockGrain>();
        private readonly ILogStore inner = A.Fake<ILogStore>();
        private readonly LockingLogStore sut;

        public LockingLogStoreTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ILockGrain>(SingleGrain.Id, null))
                .Returns(lockGrain);

            sut = new LockingLogStore(inner, grainFactory);
        }

        [Fact]
        public async Task Should_lock_and_call_inner()
        {
            var stream = new MemoryStream();

            var dateFrom = DateTime.Today;
            var dateTo = dateFrom.AddDays(2);

            var key = "MyKey";

            var releaseToken = Guid.NewGuid().ToString();

            A.CallTo(() => lockGrain.AcquireLockAsync(key))
                .Returns(releaseToken);

            await sut.ReadLockAsync(key, dateFrom, dateTo, stream);

            A.CallTo(() => lockGrain.AcquireLockAsync(key))
                .MustHaveHappened();

            A.CallTo(() => lockGrain.ReleaseLockAsync(releaseToken))
                .MustHaveHappened();

            A.CallTo(() => inner.ReadLockAsync(key, dateFrom, dateTo, stream))
                .MustHaveHappened();
        }
    }
}
