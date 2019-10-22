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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class LogSnapshotDomainObjectGrainTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly ISnapshotStore<MyDomainState, Guid> snapshotStore = A.Fake<ISnapshotStore<MyDomainState, Guid>>();
        private readonly IPersistence persistence = A.Fake<IPersistence>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyLogDomainObject sut;

        public sealed class MyLogDomainObject : LogSnapshotDomainObjectGrain<MyDomainState>
        {
            public MyLogDomainObject(IStore<Guid> store)
               : base(store, A.Dummy<ISemanticLog>())
            {
            }

            protected override Task<object> ExecuteAsync(IAggregateCommand command)
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

                return Task.FromResult<object>(null);
            }
        }

        public LogSnapshotDomainObjectGrainTests()
        {
            A.CallTo(() => store.WithEventSourcing(typeof(MyLogDomainObject), id, A<HandleEvent>.Ignored))
                .Returns(persistence);

            A.CallTo(() => store.GetSnapshotStore<MyDomainState>())
                .Returns(snapshotStore);

            sut = new MyLogDomainObject(store);
        }

        [Fact]
        public async Task Should_get_latestet_version_when_requesting_state_with_any()
        {
            await SetupUpdatedAsync();

            var result = sut.GetSnapshot(EtagVersion.Any);

            result.Should().BeEquivalentTo(new MyDomainState { Value = 8, Version = 1 });
        }

        [Fact]
        public async Task Should_get_latestet_version_when_requesting_state_with_auto()
        {
            await SetupUpdatedAsync();

            var result = sut.GetSnapshot(EtagVersion.Auto);

            result.Should().BeEquivalentTo(new MyDomainState { Value = 8, Version = 1 });
        }

        [Fact]
        public async Task Should_get_empty_version_when_requesting_state_with_empty_version()
        {
            await SetupUpdatedAsync();

            var result = sut.GetSnapshot(EtagVersion.Empty);

            result.Should().BeEquivalentTo(new MyDomainState { Value = 0, Version = -1 });
        }

        [Fact]
        public async Task Should_get_specific_version_when_requesting_state_with_specific_version()
        {
            await SetupUpdatedAsync();

            sut.GetSnapshot(0).Should().BeEquivalentTo(new MyDomainState { Value = 4, Version = 0 });
            sut.GetSnapshot(1).Should().BeEquivalentTo(new MyDomainState { Value = 8, Version = 1 });
        }

        [Fact]
        public async Task Should_get_null_state_when_requesting_state_with_invalid_version()
        {
            await SetupUpdatedAsync();

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
            await SetupEmptyAsync();

            var result = await sut.ExecuteAsync(C(new CreateAuto { Value = 4 }));

            A.CallTo(() => snapshotStore.WriteAsync(id, A<MyDomainState>.That.Matches(x => x.Value == 4), -1, 0))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();

            Assert.True(result.Value is EntityCreatedResult<Guid>);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_updated()
        {
            await SetupCreatedAsync();

            var result = await sut.ExecuteAsync(C(new UpdateAuto { Value = 8 }));

            A.CallTo(() => snapshotStore.WriteAsync(id, A<MyDomainState>.That.Matches(x => x.Value == 8), 0, 1))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();

            Assert.True(result.Value is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(8, sut.Snapshot.Value);
            Assert.Equal(1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_throw_exception_when_already_created()
        {
            await SetupCreatedAsync();

            await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(C(new CreateAuto())));
        }

        [Fact]
        public async Task Should_throw_exception_when_not_created()
        {
            await SetupEmptyAsync();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ExecuteAsync(C(new UpdateAuto())));
        }

        [Fact]
        public async Task Should_return_custom_result_on_create()
        {
            await SetupEmptyAsync();

            var result = await sut.ExecuteAsync(C(new CreateCustom()));

            Assert.Equal("CREATED", result.Value);
        }

        [Fact]
        public async Task Should_return_custom_result_on_update()
        {
            await SetupCreatedAsync();

            var result = await sut.ExecuteAsync(C(new UpdateCustom()));

            Assert.Equal("UPDATED", result.Value);
        }

        [Fact]
        public async Task Should_throw_exception_when_other_verison_expected()
        {
            await SetupCreatedAsync();

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.ExecuteAsync(C(new UpdateCustom { ExpectedVersion = 3 })));
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_create_failed()
        {
            await SetupEmptyAsync();

            A.CallTo(() => snapshotStore.WriteAsync(A<Guid>.Ignored, A<MyDomainState>.Ignored, -1, 0))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(C(new CreateAuto())));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(0,  sut.Snapshot.Value);
            Assert.Equal(-1, sut.Snapshot.Version);
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_update_failed()
        {
            await SetupCreatedAsync();

            A.CallTo(() => snapshotStore.WriteAsync(A<Guid>.Ignored, A<MyDomainState>.Ignored, 0, 1))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(C(new UpdateAuto())));

            Assert.Empty(sut.GetUncomittedEvents());

            Assert.Equal(4, sut.Snapshot.Value);
            Assert.Equal(0, sut.Snapshot.Version);
        }

        private async Task SetupCreatedAsync()
        {
            await sut.ActivateAsync(id);

            await sut.ExecuteAsync(C(new CreateAuto { Value = 4 }));
        }

        private async Task SetupUpdatedAsync()
        {
            await SetupCreatedAsync();

            await sut.ExecuteAsync(C(new UpdateAuto { Value = 8 }));
        }

        private async Task SetupEmptyAsync()
        {
            await sut.ActivateAsync(id);
        }

        private static J<IAggregateCommand> C(IAggregateCommand command)
        {
            return command.AsJ();
        }
    }
}
