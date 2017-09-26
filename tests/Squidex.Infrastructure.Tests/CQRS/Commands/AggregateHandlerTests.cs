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
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class AggregateHandlerTests
    {
        private sealed class MyEvent : IEvent
        {
        }

        private sealed class MyCommand : IAggregateCommand
        {
            public Guid AggregateId { get; set; }

            public long? ExpectedVersion { get; set; }
        }

        private sealed class MyDomainObject : DomainObjectBase
        {
            public MyDomainObject(Guid id, int version)
                : base(id, version)
            {
            }

            public MyDomainObject RaiseNewEvent(Envelope<IEvent> @event)
            {
                RaiseEvent(@event);

                return this;
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        private readonly IDomainObjectFactory factory = A.Fake<IDomainObjectFactory>();
        private readonly IDomainObjectRepository repository = A.Fake<IDomainObjectRepository>();
        private readonly Envelope<IEvent> event1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> event2 = new Envelope<IEvent>(new MyEvent());
        private readonly CommandContext context;
        private readonly AggregateHandler sut;
        private readonly MyDomainObject domainObject;

        public AggregateHandlerTests()
        {
            sut = new AggregateHandler(factory, repository);

            domainObject =
                new MyDomainObject(Guid.NewGuid(), 1)
                    .RaiseNewEvent(event1)
                    .RaiseNewEvent(event2);

            context = new CommandContext(new MyCommand { AggregateId = domainObject.Id });
        }

        [Fact]
        public Task Create_async_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.CreateAsync<MyDomainObject>(new CommandContext(A.Dummy<ICommand>()), x => TaskHelper.False));
        }

        [Fact]
        public async Task Create_async_should_create_domain_object_and_save()
        {
            A.CallTo(() => factory.CreateNew<MyDomainObject>(domainObject.Id))
                .Returns(domainObject);

            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored))
                .Returns(TaskHelper.Done);

            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, async x =>
            {
                await Task.Delay(1);

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task Create_sync_should_create_domain_object_and_save()
        {
            A.CallTo(() => factory.CreateNew<MyDomainObject>(domainObject.Id))
                .Returns(domainObject);

            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored))
                .Returns(TaskHelper.Done);

            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, x =>
            {
                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public Task Update_async_should_throw_exception_if_not_aggregate_command()
        {
            return Assert.ThrowsAnyAsync<ArgumentException>(() => sut.UpdateAsync<MyDomainObject>(new CommandContext(A.Dummy<ICommand>()), x => TaskHelper.False));
        }

        [Fact]
        public async Task Update_async_should_create_domain_object_and_save()
        {
            A.CallTo(() => factory.CreateNew<MyDomainObject>(domainObject.Id))
                .Returns(domainObject);

            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored))
                .Returns(TaskHelper.Done);

            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, async x =>
            {
                await Task.Delay(1);

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => repository.LoadAsync(domainObject, null)).MustHaveHappened();
            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task Update_sync_should_create_domain_object_and_save()
        {
            A.CallTo(() => factory.CreateNew<MyDomainObject>(domainObject.Id))
                .Returns(domainObject);

            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored))
                .Returns(TaskHelper.Done);

            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, x =>
            {
                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            A.CallTo(() => repository.LoadAsync(domainObject, null)).MustHaveHappened();
            A.CallTo(() => repository.SaveAsync(domainObject, A<ICollection<Envelope<IEvent>>>.Ignored, A<Guid>.Ignored)).MustHaveHappened();
        }
    }
}
