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
using Moq;
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

        private sealed class MyDomainObject : DomainObject
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

        private readonly Mock<IDomainObjectFactory> factory = new Mock<IDomainObjectFactory>();
        private readonly Mock<IDomainObjectRepository> repository = new Mock<IDomainObjectRepository>();
        private readonly Envelope<IEvent> event1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> event2 = new Envelope<IEvent>(new MyEvent());
        private readonly CommandContext context;
        private readonly MyCommand command;
        private readonly AggregateHandler sut;
        private readonly MyDomainObject domainObject;

        public AggregateHandlerTests()
        {
            sut = new AggregateHandler(factory.Object, repository.Object);

            domainObject =
                new MyDomainObject(Guid.NewGuid(), 1)
                    .RaiseNewEvent(event1)
                    .RaiseNewEvent(event2);

            command = new MyCommand { AggregateId = domainObject.Id };
            context = new CommandContext(command);
        }

        [Fact]
        public void Should_provide_access_to_factory()
        {
            Assert.Equal(factory.Object, sut.Factory);
        }

        [Fact]
        public void Should_provide_access_to_repository()
        {
            Assert.Equal(repository.Object, sut.Repository);
        }

        [Fact]
        public async Task Create_async_should_create_domain_object_and_save()
        {
            factory.Setup(x => x.CreateNew(typeof(MyDomainObject), domainObject.Id))
                .Returns(domainObject)
                .Verifiable();

            repository.Setup(x => x.SaveAsync(domainObject, It.IsAny<ICollection<Envelope<IEvent>>>(), It.IsAny<Guid>()))
                .Returns(TaskHelper.Done)
                .Verifiable();

            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, async x =>
            {
                await Task.Delay(1);

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            repository.VerifyAll();
        }

        [Fact]
        public async Task Create_sync_should_create_domain_object_and_save()
        {
            factory.Setup(x => x.CreateNew(typeof(MyDomainObject), domainObject.Id))
                .Returns(domainObject)
                .Verifiable();

            repository.Setup(x => x.SaveAsync(domainObject, It.IsAny<ICollection<Envelope<IEvent>>>(), It.IsAny<Guid>()))
                .Returns(TaskHelper.Done)
                .Verifiable();

            MyDomainObject passedDomainObject = null;

            await sut.CreateAsync<MyDomainObject>(context, x =>
            {
                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntityCreatedResult<Guid>>());

            repository.VerifyAll();
        }

        [Fact]
        public async Task Update_async_should_create_domain_object_and_save()
        {
            repository.Setup(x => x.GetByIdAsync<MyDomainObject>(command.AggregateId, null))
                .Returns(Task.FromResult(domainObject))
                .Verifiable();

            repository.Setup(x => x.SaveAsync(domainObject, It.IsAny<ICollection<Envelope<IEvent>>>(), It.IsAny<Guid>()))
                .Returns(TaskHelper.Done)
                .Verifiable();

            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, async x =>
            {
                await Task.Delay(1);

                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            repository.VerifyAll();
        }

        [Fact]
        public async Task Update_sync_should_create_domain_object_and_save()
        {
            repository.Setup(x => x.GetByIdAsync<MyDomainObject>(command.AggregateId, null))
                .Returns(Task.FromResult(domainObject))
                .Verifiable();

            repository.Setup(x => x.SaveAsync(domainObject, It.IsAny<ICollection<Envelope<IEvent>>>(), It.IsAny<Guid>()))
                .Returns(TaskHelper.Done)
                .Verifiable();

            MyDomainObject passedDomainObject = null;

            await sut.UpdateAsync<MyDomainObject>(context, x =>
            {
                passedDomainObject = x;
            });

            Assert.Equal(domainObject, passedDomainObject);
            Assert.NotNull(context.Result<EntitySavedResult>());

            repository.VerifyAll();
        }
    }
}
