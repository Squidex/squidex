// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class MongoExtensionsTests
    {
        public sealed class Cursor<T> : IAsyncCursor<T> where T : notnull
        {
            private readonly List<object> items = new List<object>();
            private int index = -1;

            public IEnumerable<T> Current
            {
                get
                {
                    if (items[index] is Exception ex)
                    {
                        throw ex;
                    }

                    return Enumerable.Repeat((T)items[index], 1);
                }
            }

            public Cursor<T> Add(params T[] newItems)
            {
                foreach (var item in newItems)
                {
                    items.Add(item);
                }

                return this;
            }

            public Cursor<T> Add(Exception ex)
            {
                items.Add(ex);

                return this;
            }

            public void Dispose()
            {
            }

            public bool MoveNext(CancellationToken cancellationToken = default)
            {
                index++;

                return index < items.Count;
            }

            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
            {
                await Task.Delay(1, cancellationToken);

                return MoveNext(cancellationToken);
            }
        }

        [Fact]
        public async Task Should_enumerate_over_items()
        {
            var result = new List<int>();

            var cursor = new Cursor<int>().Add(0, 1, 2, 3, 4, 5);

            await cursor.ForEachPipedAsync(x =>
            {
                result.Add(x);
                return Task.CompletedTask;
            });

            Assert.Equal(new List<int> { 0, 1, 2, 3, 4, 5 }, result);
        }

        [Fact]
        public async Task Should_break_if_cursor_failed()
        {
            var ex = new InvalidOperationException();

            var result = new List<int>();

            using (var cursor = new Cursor<int>().Add(0, 1, 2).Add(ex).Add(3, 4, 5))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                {
                    return cursor.ForEachPipedAsync(x =>
                    {
                        result.Add(x);
                        return Task.CompletedTask;
                    });
                });
            }

            Assert.Equal(new List<int> { 0, 1, 2 }, result);
        }

        [Fact]
        public async Task Should_break_if_handler_failed()
        {
            var ex = new InvalidOperationException();

            var result = new List<int>();

            using (var cursor = new Cursor<int>().Add(0, 1, 2, 3, 4, 5))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                {
                    return cursor.ForEachPipedAsync(x =>
                    {
                        if (x == 2)
                        {
                            throw ex;
                        }

                        result.Add(x);
                        return Task.CompletedTask;
                    });
                });
            }

            Assert.Equal(new List<int> { 0, 1 }, result);
        }

        [Fact]
        public async Task Should_stop_if_cancelled1()
        {
            using (var cts = new CancellationTokenSource())
            {
                var result = new List<int>();

                using (var cursor = new Cursor<int>().Add(0, 1, 2, 3, 4, 5))
                {
                    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    {
                        return cursor.ForEachPipedAsync(x =>
                        {
                            if (x == 2)
                            {
                                cts.Cancel();
                            }

                            result.Add(x);

                            return Task.CompletedTask;
                        }, cts.Token);
                    });
                }

                Assert.Equal(new List<int> { 0, 1, 2 }, result);
            }
        }
    }
}
