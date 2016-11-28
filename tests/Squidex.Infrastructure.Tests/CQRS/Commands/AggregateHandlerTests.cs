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
        private readonly Mock<IEventProcessor> processor1 = new Mock<IEventProcessor>();
        private readonly Mock<IEventProcessor> processor2 = new Mock<IEventProcessor>();
        private readonly Envelope<IEvent> event1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> event2 = new Envelope<IEvent>(new MyEvent());
        private readonly MyCommand command;
        private readonly AggregateHandler sut;
        private readonly MyDomainObject domainObject;

        public AggregateHandlerTests()
        {
            var processors = new[] { processor1.Object, processor2.Object };

            sut = new AggregateHandler(factory.Object, repository.Object, processors);

            domainObject =
                new MyDomainObject(Guid.NewGuid(), 1)
                    .RaiseNewEvent(event1)
                    .RaiseNewEvent(event2);

            command = new MyCommand { AggregateId = domainObject.Id };
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

            await TestFlowAsync(async () =>
            {
                MyDomainObject passedDomainObject = null;

                await sut.CreateAsync<MyDomainObject>(command, x =>
                {
                    passedDomainObject = x;

                    return TaskHelper.Done;
                });

                Assert.Equal(domainObject, passedDomainObject);
            });

            factory.VerifyAll();
        }

        [Fact]
        public async Task Create_sync_should_create_domain_object_and_save()
        {
            factory.Setup(x => x.CreateNew(typeof(MyDomainObject), domainObject.Id))
                .Returns(domainObject)
                .Verifiable();

            await TestFlowAsync(async () =>
            {
                MyDomainObject passedDomainObject = null;

                await sut.CreateAsync<MyDomainObject>(command, x =>
                {
                    passedDomainObject = x;
                });

                Assert.Equal(domainObject, passedDomainObject);
            });

            factory.VerifyAll();
        }

        [Fact]
        public async Task Update_async_should_create_domain_object_and_save()
        {
            repository.Setup(x => x.GetByIdAsync<MyDomainObject>(command.AggregateId, int.MaxValue))
                .Returns(Task.FromResult(domainObject))
                .Verifiable();

            await TestFlowAsync(async () =>
            {
                MyDomainObject passedDomainObject = null;

                await sut.UpdateAsync<MyDomainObject>(command, x =>
                {
                    passedDomainObject = x;

                    return TaskHelper.Done;
                });

                Assert.Equal(domainObject, passedDomainObject);
            });
        }

        [Fact]
        public async Task Update_sync_should_create_domain_object_and_save()
        {
            repository.Setup(x => x.GetByIdAsync<MyDomainObject>(command.AggregateId, int.MaxValue))
                .Returns(Task.FromResult(domainObject))
                .Verifiable();

            await TestFlowAsync(async () =>
            {
                MyDomainObject passedDomainObject = null;

                await sut.UpdateAsync<MyDomainObject>(command, x =>
                {
                    passedDomainObject = x;
                });

                Assert.Equal(domainObject, passedDomainObject);
            });
        }

        private async Task TestFlowAsync(Func<Task> action)
        {
            repository.Setup(x => x.SaveAsync(domainObject,
                    It.IsAny<ICollection<Envelope<IEvent>>>(),
                    It.IsAny<Guid>()))
                .Returns(TaskHelper.Done)
                .Verifiable();

            processor1.Setup(x => x.ProcessEventAsync(
                    It.Is<Envelope<IEvent>>(y => y.Payload == event1.Payload), domainObject, command))
                .Returns(TaskHelper.Done)
                .Verifiable();

            processor2.Setup(x => x.ProcessEventAsync(
                    It.Is<Envelope<IEvent>>(y => y.Payload == event1.Payload), domainObject, command))
                .Returns(TaskHelper.Done)
                .Verifiable();

            processor1.Setup(x => x.ProcessEventAsync(
                    It.Is<Envelope<IEvent>>(y => y.Payload == event2.Payload), domainObject, command))
                .Returns(TaskHelper.Done)
                .Verifiable();

            processor2.Setup(x => x.ProcessEventAsync(
                    It.Is<Envelope<IEvent>>(y => y.Payload == event2.Payload), domainObject, command))
                .Returns(TaskHelper.Done)
                .Verifiable();

            await action();

            processor1.VerifyAll();
            processor2.VerifyAll();

            repository.VerifyAll();
        }
    }
}
