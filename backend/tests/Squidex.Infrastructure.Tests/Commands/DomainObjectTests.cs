// ==========================================================================
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
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Log;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectTests
    {
        private readonly IStore<DomainId> store = A.Fake<IStore<DomainId>>();
        private readonly IPersistence<MyDomainState> persistence = A.Fake<IPersistence<MyDomainState>>();
        private readonly DomainId id = DomainId.NewGuid();
        private readonly MyDomainObject sut;

        public sealed class MyDomainObject : DomainObject<MyDomainState>
        {
            public MyDomainObject(IStore<DomainId> store)
               : base(store, A.Dummy<ISemanticLog>())
            {
            }

            protected override bool CanAcceptCreation(ICommand command)
            {
                if (command is CreateAuto update)
                {
                    return update.Value != 99;
                }

                return true;
            }

            protected override bool CanAccept(ICommand command)
            {
                if (command is UpdateAuto update)
                {
                    return update.Value != 99;
                }

                return true;
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
            SetupEmpty();

            var result = await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>._))
                .MustNotHaveHappened();

            Assert.True(result is EntityCreatedResult<DomainId>);

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

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>._))
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

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>._))
                .MustHaveHappenedOnceExactly();

            Assert.True(result is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(8, sut.Snapshot.Value);
            Assert.Equal(1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_load_on_create()
        {
            SetupEmpty();

            await sut.ExecuteAsync(new CreateAuto());
        }

        [Fact]
        public async Task Should_load_once_on_update()
        {
            SetupCreated(4);

            await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 9, ExpectedVersion = 1 });

            A.CallTo(() => persistence.ReadAsync(A<long>._))
                .MustHaveHappenedOnceExactly();

            Assert.Equal(9, sut.Snapshot.Value);
            Assert.Equal(2, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_rebuild_state()
        {
            SetupCreated(4);

            await sut.RebuildStateAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_throw_on_rebuild_when_no_event_found()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RebuildStateAsync());
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
        public async Task Should_throw_exception_when_already_created()
        {
            SetupEmpty();

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>._))
                .Throws(new InconsistentStateException(4, EtagVersion.NotFound));

            await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto()));
        }

        [Fact]
        public async Task Should_throw_exception_when_already_created_after_creation()
        {
            await sut.ExecuteAsync(new CreateAuto());

            await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto()));
        }

        [Fact]
        public async Task Should_throw_exception_when_not_created()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ExecuteAsync(new UpdateAuto()));
        }

        [Fact]
        public async Task Should_throw_exception_when_create_command_not_accepted()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(new CreateAuto { Value = 99 }));
        }

        [Fact]
        public async Task Should_throw_exception_when_update_command_not_accepted()
        {
            SetupCreated(4);

            await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(new UpdateAuto { Value = 99 }));
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

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>._))
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

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>._))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new UpdateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
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

            A.CallTo(() => store.WithSnapshotsAndEventSourcing(typeof(MyDomainObject), id, A<HandleSnapshot<MyDomainState>>._, A<HandleEvent>._))
                .Invokes(args =>
                {
                    handleEvent = args.GetArgument<HandleEvent>(3)!;
                })
                .Returns(persistence);

            A.CallTo(() => persistence.Version)
                .ReturnsLazily(() => version);

            sut.Setup(id);
        }

        private void SetupEmpty()
        {
            A.CallTo(() => store.WithSnapshotsAndEventSourcing(typeof(MyDomainObject), id, A<HandleSnapshot<MyDomainState>>._, A<HandleEvent>._))
                .Returns(persistence);

            A.CallTo(() => persistence.Version)
                .Returns(-1);

            sut.Setup(id);
        }
    }
}
