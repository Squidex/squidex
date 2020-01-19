﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<MyDomainState> persistence = A.Fake<IPersistence<MyDomainState>>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyDomainObject sut;

        public sealed class MyDomainObject : DomainObject<MyDomainState>
        {
            public MyDomainObject(IStore<Guid> store)
               : base(store, A.Dummy<ISemanticLog>())
            {
            }

            public override Task<object?> ExecuteAsync(IAggregateCommand command)
            {
                switch (command)
                {
                    case CreateAuto createAuto:
                        return Create(createAuto, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });
                        });

                    case CreateCustom createCustom:
                        return CreateReturn(createCustom, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });

                            return "CREATED";
                        });

                    case UpdateAuto updateAuto:
                        return Update(updateAuto, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });
                        });

                    case UpdateCustom updateCustom:
                        return UpdateReturn(updateCustom, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });

                            return "UPDATED";
                        });
                }

                return Task.FromResult<object?>(null);
            }
        }

        public DomainObjectTests()
        {
            A.CallTo(() => store.WithSnapshotsAndEventSourcing(typeof(MyDomainObject), id, A<HandleSnapshot<MyDomainState>>.Ignored, A<HandleEvent>.Ignored))
                .Returns(persistence);

            sut = new MyDomainObject(store);
        }

        [Fact]
        public void Should_instantiate()
        {
            Assert.Equal(EtagVersion.Empty, sut.Version);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_created()
        {
            await SetupEmptyAsync();

            var result = await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();

            Assert.True(result is EntityCreatedResult<Guid>);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_updated()
        {
            await SetupCreatedAsync();

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = 8 });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();

            Assert.True(result is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(8, sut.Snapshot.Value);
            Assert.Equal(1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_rebuild_state_async()
        {
            await SetupCreatedAsync();

            await sut.RebuildStateAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_update_when_snapshot_is_not_changed()
        {
            await SetupCreatedAsync();

            var previousSnapshot = sut.Snapshot;

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = MyDomainState.Unchanged });

            Assert.True(result is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);

            Assert.Same(previousSnapshot, sut.Snapshot);
        }

        [Fact]
        public async Task Should_not_throw_exception_when_already_created()
        {
            await SetupCreatedAsync();

            await sut.ExecuteAsync(new CreateAuto());
        }

        [Fact]
        public async Task Should_throw_exception_when_not_created()
        {
            await SetupEmptyAsync();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ExecuteAsync(new UpdateAuto()));
        }

        [Fact]
        public async Task Should_return_custom_result_on_create()
        {
            await SetupEmptyAsync();

            var result = await sut.ExecuteAsync(new CreateCustom());

            Assert.Equal("CREATED", result);
        }

        [Fact]
        public async Task Should_return_custom_result_on_update()
        {
            await SetupCreatedAsync();

            var result = await sut.ExecuteAsync(new UpdateCustom());

            Assert.Equal("UPDATED", result);
        }

        [Fact]
        public async Task Should_throw_exception_when_other_verison_expected()
        {
            await SetupCreatedAsync();

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.ExecuteAsync(new UpdateCustom { ExpectedVersion = 3 }));
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_create_failed()
        {
            await SetupEmptyAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.Ignored))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new CreateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(0,  sut.Snapshot.Value);
            Assert.Equal(-1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_update_failed()
        {
            await SetupCreatedAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.Ignored))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new UpdateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        private async Task SetupCreatedAsync()
        {
            sut.Setup(id);

            await sut.ExecuteAsync(new CreateAuto { Value = 4 });
        }

        private async Task SetupEmptyAsync()
        {
            sut.Setup(id);

            await Task.Yield();
        }
    }
}
