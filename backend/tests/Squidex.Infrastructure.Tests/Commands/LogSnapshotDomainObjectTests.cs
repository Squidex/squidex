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
using FluentAssertions;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class LogSnapshotDomainObjectTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly ISnapshotStore<MyDomainState, Guid> snapshotStore = A.Fake<ISnapshotStore<MyDomainState, Guid>>();
        private readonly IPersistence persistence = A.Fake<IPersistence>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyLogDomainObject sut;

        public sealed class MyLogDomainObject : LogSnapshotDomainObject<MyDomainState>
        {
            public MyLogDomainObject(IStore<Guid> store)
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

        public LogSnapshotDomainObjectTests()
        {
            A.CallTo(() => store.GetSnapshotStore<MyDomainState>())
                .Returns(snapshotStore);

            sut = new MyLogDomainObject(store);
        }

        [Fact]
        public async Task Should_get_latestet_version_when_requesting_state_with_any()
        {
            await SetupUpdatedAsync(4, 8);

            var result = sut.GetSnapshot(EtagVersion.Any);

            result.Should().BeEquivalentTo(new MyDomainState { Value = 8, Version = 1 });
        }

        [Fact]
        public async Task Should_get_latestet_version_when_requesting_state_with_auto()
        {
            await SetupUpdatedAsync(4, 8);

            var result = sut.GetSnapshot(EtagVersion.Auto);

            result.Should().BeEquivalentTo(new MyDomainState { Value = 8, Version = 1 });
        }

        [Fact]
        public async Task Should_get_empty_version_when_requesting_state_with_empty_version()
        {
            await SetupUpdatedAsync(4, 8);

            var result = sut.GetSnapshot(EtagVersion.Empty);

            result.Should().BeEquivalentTo(new MyDomainState { Value = 0, Version = -1 });
        }

        [Fact]
        public async Task Should_get_specific_version_when_requesting_state_with_specific_version()
        {
            await SetupUpdatedAsync(4, 8);

            sut.GetSnapshot(0).Should().BeEquivalentTo(new MyDomainState { Value = 4, Version = 0 });
            sut.GetSnapshot(1).Should().BeEquivalentTo(new MyDomainState { Value = 8, Version = 1 });
        }

        [Fact]
        public async Task Should_get_null_state_when_requesting_state_with_invalid_version()
        {
            await SetupUpdatedAsync(4, 8);

            Assert.Null(sut.GetSnapshot(-4));
            Assert.Null(sut.GetSnapshot(2));
        }

        [Fact]
        public void Should_instantiate()
        {
            Assert.Equal(EtagVersion.Empty, sut.Version);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_created()
        {
            SetupEmpty();

            var result = await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            A.CallTo(() => snapshotStore.WriteAsync(id, A<MyDomainState>.That.Matches(x => x.Value == 4), -1, 0))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>.Ignored))
                .MustNotHaveHappened();

            Assert.True(result is EntityCreatedResult<Guid>);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_updated_after_creation()
        {
            SetupEmpty();

            await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });

            A.CallTo(() => snapshotStore.WriteAsync(id, A<MyDomainState>.That.Matches(x => x.Value == 8), 0, 1))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>.Ignored))
                .MustNotHaveHappened();

            Assert.True(result is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(8, sut.Snapshot.Value);
            Assert.Equal(1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_updated()
        {
            SetupCreated(4);

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });

            A.CallTo(() => snapshotStore.WriteAsync(id, A<MyDomainState>.That.Matches(x => x.Value == 8), 0, 1))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>.Ignored))
                .MustHaveHappenedOnceExactly();

            Assert.True(result is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(8, sut.Snapshot.Value);
            Assert.Equal(1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_only_load_once_on_update()
        {
            SetupCreated(4);

            await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 9, ExpectedVersion = 1 });

            A.CallTo(() => persistence.ReadAsync(A<long>.Ignored))
                .MustHaveHappenedOnceExactly();

            Assert.Equal(9, sut.Snapshot.Value);
            Assert.Equal(2, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_rebuild_state_async()
        {
            SetupCreated(4);

            await sut.RebuildStateAsync();

            A.CallTo(() => snapshotStore.WriteAsync(id, A<MyDomainState>.That.Matches(x => x.Value == 4), EtagVersion.Any, 0))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_update_when_snapshot_is_not_changed()
        {
            SetupCreated(4);

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = MyDomainState.Unchanged });

            Assert.True(result is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_not_throw_exception_when_already_created()
        {
            SetupCreated(4);

            await sut.ExecuteAsync(new CreateAuto());
        }

        [Fact]
        public async Task Should_throw_exception_when_not_created()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ExecuteAsync(new UpdateAuto()));
        }

        [Fact]
        public async Task Should_return_custom_result_on_create()
        {
            SetupEmpty();

            var result = await sut.ExecuteAsync(new CreateCustom());

            Assert.Equal("CREATED", result);
        }

        [Fact]
        public async Task Should_return_custom_result_on_update()
        {
            SetupCreated(4);

            var result = await sut.ExecuteAsync(new UpdateCustom());

            Assert.Equal("UPDATED", result);
        }

        [Fact]
        public async Task Should_throw_exception_when_other_verison_expected()
        {
            SetupCreated(4);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.ExecuteAsync(new UpdateCustom { ExpectedVersion = 3 }));
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_create_failed()
        {
            SetupEmpty();

            A.CallTo(() => snapshotStore.WriteAsync(A<Guid>.Ignored, A<MyDomainState>.Ignored, -1, 0))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new CreateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(0,  sut.Snapshot.Value);
            Assert.Equal(-1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_update_failed()
        {
            SetupCreated(4);

            A.CallTo(() => snapshotStore.WriteAsync(A<Guid>.Ignored, A<MyDomainState>.Ignored, 0, 1))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new UpdateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        private async Task SetupUpdatedAsync(int value, int updatedValue)
        {
            SetupCreated(value);

            await sut.ExecuteAsync(new UpdateAuto { Value = updatedValue });
        }

        private void SetupCreated(int value)
        {
            HandleEvent handleEvent = x => { };

            var version = -1;

            A.CallTo(() => persistence.ReadAsync(-2))
                .Invokes(() =>
                {
                    version = 0;

                    handleEvent(Envelope.Create(new ValueChanged { Value = value }));
                });

            A.CallTo(() => store.WithEventSourcing(typeof(MyLogDomainObject), id, A<HandleEvent>.Ignored))
                .Invokes(args =>
                {
                    handleEvent = args.GetArgument<HandleEvent>(2)!;
                })
                .Returns(persistence);

            A.CallTo(() => persistence.Version)
                .ReturnsLazily(() => version);

            sut.Setup(id);
        }

        private void SetupEmpty()
        {
            A.CallTo(() => store.WithEventSourcing(typeof(MyLogDomainObject), id, A<HandleEvent>.Ignored))
                .Returns(persistence);

            A.CallTo(() => persistence.Version)
                .Returns(-1);

            sut.Setup(id);
        }
    }
}
