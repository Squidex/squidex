// ==========================================================================
//  AggregateHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class AggregateHandlerTests
    {
        private readonly IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IStateFactory stateFactory = A.Fake<IStateFactory>();
        private readonly IPersistence<MyDomainState> persistence = A.Fake<IPersistence<MyDomainState>>();
        private readonly Envelope<IEvent> event1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> event2 = new Envelope<IEvent>(new MyEvent());
        private readonly CommandContext context;
        private readonly CommandContext invalidContext = new CommandContext(A.Dummy<ICommand>());
        private readonly Guid domainObjectId = Guid.NewGuid();
        private readonly MyCommand command;
        private readonly MyDomainObject domainObject = new MyDomainObject();
        private readonly AggregateHandler sut;

        public AggregateHandlerTests()
        {
            command = new MyCommand { AggregateId = domainObjectId, ExpectedVersion = EtagVersion.Any };
            context = new CommandContext(command);

            A.CallTo(() => store.WithSnapshotsAndEventSourcing(domainObjectId, A<Func<MyDomainState, Task>>.Ignored, A<Func<Envelope<IEvent>, Task>>.Ignored))
                .Returns(persistence);

            A.CallTo(() => stateFactory.CreateAsync<MyDomainObject>(domainObjectId))
                .Returns(Task.FromResult(domainObject));

            A.CallTo(() => stateFactory.GetSingleAsync<MyDomainObject>(domainObjectId))
                .Returns(Task.FromResult(domainObject));

            sut = new AggregateHandler(stateFactory, serviceProvider);

            domainObject.ActivateAsync(domainObjectId, store).Wait();
        }

        [Fact]
        public Task Create_with_task_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.CreateAsync<MyDomainObject>(invalidContext, x => TaskHelper.False));
        }

        [Fact]
        public Task Create_synced_with_task_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.CreateSyncedAsync<MyDomainObject>(invalidContext, x => TaskHelper.False));
        }

        [Fact]
        public Task Create_with_task_should_should_throw_exception_if_version_is_wrong()
        {
            command.ExpectedVersion = 2;

            return Assert.ThrowsAnyAsync<DomainObjectVersionException>(() => sut.CreateAsync<MyDomainObject>(context, x => TaskHelper.False));
        }

        [Fact]
        public Task Create_synced_with_task_should_should_throw_exception_if_version_is_wrong()
        {
            command.ExpectedVersion = 2;

            return Assert.ThrowsAnyAsync<DomainObjectVersionException>(() => sut.CreateSyncedAsync<MyDomainObject>(context, x => TaskHelper.False));
        }

        [Fact]
        public async Task Create_with_task_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, async x =>
            {
                x.RaiseEvent(new MyEvent());

                await Task.Yield();

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_synced_with_task_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.CreateSyncedAsync<MyDomainObject>(context, async x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                await Task.Yield();

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_synced_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.CreateSyncedAsync<MyDomainObject>(context, x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public Task Update_with_task_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.UpdateAsync<MyDomainObject>(invalidContext, x => TaskHelper.False));
        }

        [Fact]
        public Task Update_synced_with_task_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.UpdateSyncedAsync<MyDomainObject>(invalidContext, x => TaskHelper.False));
        }

        [Fact]
        public Task Update_with_task_should_should_throw_exception_if_version_is_wrong()
        {
            command.ExpectedVersion = 2;

            return Assert.ThrowsAnyAsync<DomainObjectVersionException>(() => sut.UpdateAsync<MyDomainObject>(context, x => TaskHelper.False));
        }

        [Fact]
        public Task Update_synced_with_task_should_should_throw_exception_if_version_is_wrong()
        {
            command.ExpectedVersion = 2;

            return Assert.ThrowsAnyAsync<DomainObjectVersionException>(() => sut.UpdateSyncedAsync<MyDomainObject>(context, x => TaskHelper.False));
        }

        [Fact]
        public async Task Update_with_task_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, async x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                await Task.Yield();

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_synced_with_task_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.UpdateSyncedAsync<MyDomainObject>(context, async x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                await Task.Yield();

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_synced_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.UpdateSyncedAsync<MyDomainObject>(context, x =>
            {
                x.RaiseEvent(new MyEvent());
                x.RaiseEvent(new MyEvent());

                passedDomainObject = x;
            });

            Assert.Equal(1, domainObject.Snapshot.Version);
            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .MustHaveHappened();
        }
    }
}
