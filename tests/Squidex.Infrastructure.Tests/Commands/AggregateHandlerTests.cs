// ==========================================================================
//  AggregateHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Commands.TestHelpers;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class AggregateHandlerTests
    {
        private readonly IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        private readonly IStore store = A.Fake<IStore>();
        private readonly IStateFactory stateFactory = A.Fake<IStateFactory>();
        private readonly IPersistence<object> persistence = A.Fake<IPersistence<object>>();
        private readonly Envelope<IEvent> event1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> event2 = new Envelope<IEvent>(new MyEvent());
        private readonly DomainObjectFactoryFunction<MyDomainObject> factory;
        private readonly CommandContext context;
        private readonly AggregateHandler sut;
        private readonly DomainObjectWrapper<MyDomainObject> domainObjectWrapper = new DomainObjectWrapper<MyDomainObject>();
        private readonly Guid domainObjectId = Guid.NewGuid();
        private readonly MyDomainObject domainObject;

        public AggregateHandlerTests()
        {
            factory = new DomainObjectFactoryFunction<MyDomainObject>(id => domainObject);

            domainObject =
                new MyDomainObject(domainObjectId, 1)
                    .RaiseNewEvent(event1)
                    .RaiseNewEvent(event2);

            context = new CommandContext(new MyCommand { AggregateId = domainObject.Id });

            A.CallTo(() => store.WithEventSourcing<MyDomainObject>(domainObjectId.ToString(), A<Func<Envelope<IEvent>, Task>>.Ignored))
                .Returns(persistence);

            A.CallTo(() => serviceProvider.GetService(factory.GetType()))
                .Returns(factory);

            A.CallTo(() => stateFactory.GetDetachedAsync<DomainObjectWrapper<MyDomainObject>>(domainObject.Id.ToString()))
                .Returns(Task.FromResult(domainObjectWrapper));

            sut = new AggregateHandler(stateFactory, serviceProvider);

            domainObjectWrapper.ActivateAsync(domainObjectId.ToString(), store).Wait();
        }

        [Fact]
        public Task Create_async_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.CreateAsync<MyDomainObject>(new CommandContext(A.Dummy<ICommand>()), x => TaskHelper.False));
        }

        [Fact]
        public async Task Create_async_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, async x =>
            {
                await Task.Yield();

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => persistence.ReadAsync(-1))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteEventsAsync(A<Envelope<IEvent>[]>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_sync_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, x =>
            {
                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => persistence.ReadAsync(-1))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteEventsAsync(A<Envelope<IEvent>[]>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public Task Update_async_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.UpdateAsync<MyDomainObject>(new CommandContext(A.Dummy<ICommand>()), x => TaskHelper.False));
        }

        [Fact]
        public async Task Update_async_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, async x =>
            {
                await Task.Yield();

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => persistence.ReadAsync(null))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteEventsAsync(A<Envelope<IEvent>[]>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_sync_should_create_domain_object_and_save()
        {
            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, x =>
            {
                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => persistence.ReadAsync(null))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteEventsAsync(A<Envelope<IEvent>[]>.Ignored))
                .MustHaveHappened();
        }
    }
}
