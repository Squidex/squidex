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
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.MongoDb
{
    public class MongoExtensionsTests
    {
        public sealed class Cursor<T> : IAsyncCursor<T>
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

            public Cursor<T> Add(T item)
            {
                items.Add(item);

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

            public bool MoveNext(CancellationToken cancellationToken = default(CancellationToken))
            {
                index++;

                return index < items.Count;
            }

            public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                await Task.Delay(1, cancellationToken);

                return MoveNext(cancellationToken);
            }
        }

        [Fact]
        public async Task Should_enumerate_over_items()
        {
            var result = new List<int>();

            var cursor = new Cursor<int>().Add(0).Add(1).Add(1).Add(2).Add(3).Add(5);

            await cursor.ForEachPipelineAsync(x =>
            {
                result.Add(x);
                return TaskHelper.Done;
            });

            Assert.Equal(new List<int> { 0, 1, 1, 2, 3, 5 }, result);
        }

        [Fact]
        public async Task Should_break_when_cursor_failed()
        {
            var ex = new InvalidOperationException();

            var result = new List<int>();

            var cursor = new Cursor<int>().Add(0).Add(1).Add(1).Add(ex).Add(2).Add(3).Add(5);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return cursor.ForEachPipelineAsync(x =>
                {
                    result.Add(x);
                    return TaskHelper.Done;
                });
            });

            Assert.Equal(new List<int> { 0, 1, 1 }, result);
        }

        [Fact]
        public async Task Should_break_when_handler_failed()
        {
            var ex = new InvalidOperationException();

            var result = new List<int>();

            var cursor = new Cursor<int>().Add(0).Add(1).Add(1).Add(2).Add(3).Add(5);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return cursor.ForEachPipelineAsync(x =>
                {
                    if (x == 2)
                    {
                        throw ex;
                    }

                    result.Add(x);
                    return TaskHelper.Done;
                });
            });

            Assert.Equal(new List<int> { 0, 1, 1 }, result);
        }

        [Fact]
        public async Task Should_stop_when_cancelled1()
        {
            var cts = new CancellationTokenSource();

            var result = new List<int>();

            var cursor = new Cursor<int>().Add(0).Add(1).Add(1).Add(2).Add(3).Add(5);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
            {
                return cursor.ForEachPipelineAsync(x =>
                {
                    if (x == 2)
                    {
                        cts.Cancel();
                    }

                    result.Add(x);

                    return TaskHelper.Done;
                }, cts.Token);
            });

            Assert.Equal(new List<int> { 0, 1, 1, 2 }, result);
        }
    }
}
