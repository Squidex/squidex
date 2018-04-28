// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Tasks
{
    public class AsyncLockTests
    {
        [Fact]
        public async Task Should_lock()
        {
            var sut = new AsyncLock();

            var value = 0;

            await Task.WhenAll(
                Enumerable.Repeat(0, 100).Select(x => new Func<Task>(async () =>
                {
                    using (await sut.LockAsync())
                    {
                        await Task.Yield();

                        value++;
                    }
                })()));

            Assert.Equal(100, value);
        }
    }
}
