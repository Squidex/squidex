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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public sealed class DomainObjectGrainTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<MyDomainState> persistence = A.Fake<IPersistence<MyDomainState>>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyDomainObject sut;

        public sealed class MyDomainState : IDomainState
        {
            public long Version { get; set; }

            public int Value { get; set; }
        }

        public sealed class ValueChanged : IEvent
        {
            public int Value { get; set; }
        }

        public sealed class CreateAuto : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class CreateCustom : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class UpdateAuto : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class UpdateCustom : MyCommand
        {
            public int Value { get; set; }
        }

        public sealed class MyDomainObject : DomainObjectGrain<MyDomainState>
        {
            public MyDomainObject(IStore<Guid> store)
               : base(store)
            {
            }

            protected override Task<object> ExecuteAsync(IAggregateCommand command)
            {
                switch (command)
                {
                    case CreateAuto createAuto:
                        return CreateAsync(createAuto, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });
                        });

                    case CreateCustom createCustom:
                        return CreateReturnAsync(createCustom, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });

                            return "CREATED";
                        });

                    case UpdateAuto updateAuto:
                        return UpdateAsync(updateAuto, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });
                        });

                    case UpdateCustom updateCustom:
                        return UpdateReturnAsync(updateCustom, c =>
                        {
                            RaiseEvent(new ValueChanged { Value = c.Value });

                            return "UPDATED";
                        });
                }

                return Task.FromResult<object>(null);
            }

            public override void ApplyEvent(Envelope<IEvent> @event)
            {
                if (@event.Payload is ValueChanged valueChanged)
                {
                    ApplySnapshot(new MyDomainState { Value = valueChanged.Value });
                }
            }
        }

        public DomainObjectGrainTests()
        {
            A.CallTo(() => store.WithSnapshotsAndEventSourcing(typeof(MyDomainObject), id, A<Func<MyDomainState, Task>>.Ignored, A<Func<Envelope<IEvent>, Task>>.Ignored))
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

            var result = await sut.ExecuteAsync(C(new CreateAuto { Value = 5 }));

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 5)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();

            Assert.True(result.Value is EntityCreatedResult<Guid>);

            Assert.Empty(sut.GetUncomittedEvents());
            Assert.Equal(5, sut.Snapshot.Value);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_updated()
        {
            await SetupCreatedAsync();

            var result = await sut.ExecuteAsync(C(new UpdateAuto { Value = 5 }));

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 5)))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 1)))
                .MustHaveHappened();

            Assert.True(result.Value is EntitySavedResult);

            Assert.Empty(sut.GetUncomittedEvents());
            Assert.Equal(5, sut.Snapshot.Value);
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

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.Ignored))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(C(new CreateAuto())));

            Assert.Empty(sut.GetUncomittedEvents());
            Assert.Equal(0, sut.Snapshot.Value);
        }

        [Fact]
        public async Task Should_reset_state_when_writing_snapshot_for_update_failed()
        {
            await SetupCreatedAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.Ignored))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(C(new UpdateAuto())));

            Assert.Empty(sut.GetUncomittedEvents());
            Assert.Equal(0, sut.Snapshot.Value);
        }

        private async Task SetupCreatedAsync()
        {
            await sut.OnActivateAsync(id);

            await sut.ExecuteAsync(C(new CreateAuto()));
        }

        private static J<IAggregateCommand> C(IAggregateCommand command)
        {
            return command.AsJ();
        }

        private async Task SetupEmptyAsync()
        {
            await sut.OnActivateAsync(id);
        }
    }
}
