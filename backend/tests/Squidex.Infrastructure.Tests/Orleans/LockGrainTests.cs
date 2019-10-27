// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class LockGrainTests
    {
        private readonly LockGrain sut = new LockGrain();

        [Fact]
        public async Task Should_not_acquire_lock_when_locked()
        {
            var releaseLock1 = await sut.AcquireLockAsync("Key1");
            var releaseLock2 = await sut.AcquireLockAsync("Key1");

            Assert.NotNull(releaseLock1);
            Assert.Null(releaseLock2);
        }

        [Fact]
        public async Task Should_acquire_lock_with_other_key()
        {
            var releaseLock1 = await sut.AcquireLockAsync("Key1");
            var releaseLock2 = await sut.AcquireLockAsync("Key2");

            Assert.NotNull(releaseLock1);
            Assert.NotNull(releaseLock2);
        }

        [Fact]
        public async Task Should_acquire_lock_after_released()
        {
            var releaseLock1 = await sut.AcquireLockAsync("Key1");

            await sut.ReleaseLockAsync(releaseLock1!);

            var releaseLock2 = await sut.AcquireLockAsync("Key1");

            Assert.NotNull(releaseLock1);
            Assert.NotNull(releaseLock2);
        }
    }
}
