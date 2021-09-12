﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class PersistenceSnapshotTests
    {
        private readonly DomainId key = DomainId.NewGuid();
        private readonly ISnapshotStore<int> snapshotStore = A.Fake<ISnapshotStore<int>>();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly IStore<int> sut;

        public PersistenceSnapshotTests()
        {
            sut = new Store<int>(snapshotStore, eventStore, eventDataFormatter, streamNameResolver);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((20, true, 10));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.ReadAsync();

            Assert.Equal(10, persistence.Version);
            Assert.Equal(20, persistedState.Value);
        }

        [Fact]
        public async Task Should_not_read_from_store_if_not_valid()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((20, false, 10));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.ReadAsync();

            Assert.Equal(10, persistence.Version);
            Assert.Equal(0, persistedState.Value);
        }

        [Fact]
        public async Task Should_return_empty_version_if_version_negative()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((20, true, -10));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.ReadAsync();

            Assert.Equal(EtagVersion.Empty, persistence.Version);
        }

        [Fact]
        public async Task Should_set_to_empty_if_store_returns_not_found()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((20, true, EtagVersion.Empty));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.ReadAsync();

            Assert.Equal(-1, persistence.Version);
            Assert.Equal(0, persistedState.Value);
        }

        [Fact]
        public async Task Should_throw_exception_if_not_found_and_version_expected()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((123, true, EtagVersion.Empty));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((123, true, 2));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await Assert.ThrowsAsync<InconsistentStateException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_version()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((20, true, 10));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.ReadAsync();

            Assert.Equal(10, persistence.Version);
            Assert.Equal(20, persistedState.Value);

            await persistence.WriteSnapshotAsync(100);

            A.CallTo(() => snapshotStore.WriteAsync(key, 100, 10, 11, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_write_snapshot_to_store_with_empty_version()
        {
            var persistence = sut.WithSnapshots(None.Type, key, null);

            await persistence.WriteSnapshotAsync(100);

            A.CallTo(() => snapshotStore.WriteAsync(key, 100, EtagVersion.Empty, 0, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_wrap_exception_if_writing_to_store_with_previous_version()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key, A<CancellationToken>._))
                .Returns((20, true, 10));

            A.CallTo(() => snapshotStore.WriteAsync(key, 100, 10, 11, A<CancellationToken>._))
                .Throws(new InconsistentStateException(1, 1, new InvalidOperationException()));

            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.ReadAsync();

            await Assert.ThrowsAsync<InconsistentStateException>(() => persistence.WriteSnapshotAsync(100));
        }

        [Fact]
        public async Task Should_delete_snapshot_but_not_events_if_deleted()
        {
            var persistedState = Save.Snapshot(0);
            var persistence = sut.WithSnapshots(None.Type, key, persistedState.Write);

            await persistence.DeleteAsync();

            A.CallTo(() => eventStore.DeleteStreamAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => snapshotStore.RemoveAsync(key, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_snapshot_store_on_clear()
        {
            await sut.ClearSnapshotsAsync();

            A.CallTo(() => snapshotStore.ClearAsync(A<CancellationToken>._))
                .MustHaveHappened();
        }
    }
}
