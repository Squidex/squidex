// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex.Infrastructure.States
{
    public class PersistenceSnapshotTests
    {
        private readonly string key = Guid.NewGuid().ToString();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly ISnapshotStore<int, string> snapshotStore = A.Fake<ISnapshotStore<int, string>>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly IStore<string> sut;

        public PersistenceSnapshotTests()
        {
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<int, string>)))
                .Returns(snapshotStore);

            sut = new Store<string>(eventStore, eventDataFormatter, services, streamNameResolver);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((20, 10));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await persistence.ReadAsync();

            Assert.Equal(10, persistence.Version);
            Assert.Equal(20, persistedState);
        }

        [Fact]
        public async Task Should_return_empty_version_when_version_negative()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((20, -10));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await persistence.ReadAsync();

            Assert.Equal(EtagVersion.Empty, persistence.Version);
        }

        [Fact]
        public async Task Should_set_to_empty_when_store_returns_not_found()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((20, EtagVersion.Empty));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await persistence.ReadAsync();

            Assert.Equal(-1, persistence.Version);
            Assert.Equal( 0, persistedState);
        }

        [Fact]
        public async Task Should_throw_exception_if_not_found_and_version_expected()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, EtagVersion.Empty));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, 2));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_version()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((20, 10));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await persistence.ReadAsync();

            Assert.Equal(10, persistence.Version);
            Assert.Equal(20, persistedState);

            await persistence.WriteSnapshotAsync(100);

            A.CallTo(() => snapshotStore.WriteAsync(key, 100, 10, 11))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wrap_exception_when_writing_to_store_with_previous_version()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((20, 10));

            A.CallTo(() => snapshotStore.WriteAsync(key, 100, 10, 11))
                .Throws(new InconsistentStateException(1, 1, new InvalidOperationException()));

            var persistedState = 0;
            var persistence = sut.WithSnapshots<object, int, string>(key, x => persistedState = x);

            await persistence.ReadAsync();

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => persistence.WriteSnapshotAsync(100));
        }

        [Fact]
        public async Task Should_delete_snapshot_but_not_events_when_deleted()
        {
            var persistence = sut.WithSnapshots<object, int, string>(key, x => { });

            await persistence.DeleteAsync();

            A.CallTo(() => eventStore.DeleteStreamAsync(A<string>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => snapshotStore.RemoveAsync(key))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_snapshot_store_on_clear()
        {
            await sut.ClearSnapshotsAsync<string, int>();

            A.CallTo(() => snapshotStore.ClearAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_snapshot_but_not_events_when_deleted_from_store()
        {
            await sut.RemoveSnapshotAsync<string, int>(key);

            A.CallTo(() => eventStore.DeleteStreamAsync(A<string>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => snapshotStore.RemoveAsync(key))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_get_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, -1));

            var result = await sut.GetSnapshotAsync<string, int>(key);

            Assert.Equal(123, result);
        }
    }
}