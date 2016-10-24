// ==========================================================================
//  CommandsHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class CommandsHandlerTests
    {
        private readonly Mock<IDomainObjectFactory> domainObjectFactory = new Mock<IDomainObjectFactory>();
        private readonly Mock<IDomainObjectRepository> domainObjectRepository = new Mock<IDomainObjectRepository>();
        private readonly TestCommandHandler sut;
        private readonly Guid id = Guid.NewGuid();

        private sealed class TestCommand : AggregateCommand
        {
        }

        private sealed class TestDomainObject : DomainObject
        {
            public TestDomainObject(Guid id, int version) : base(id, version)
            {
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class TestCommandHandler : CommandHandler<TestDomainObject>
        {
            public TestCommandHandler(IDomainObjectFactory domainObjectFactory, IDomainObjectRepository domainObjectRepository) 
                : base(domainObjectFactory, domainObjectRepository)
            {
            }

            public override Task<bool> HandleAsync(CommandContext context)
            {
                throw new NotImplementedException();
            }

            public IDomainObjectFactory TestFactory
            {
                get { return Factory; }
            }

            public IDomainObjectRepository TestRepository
            {
                get { return Repository; }
            }

            public Task CreateTestAsync(IAggregateCommand command, Action<TestDomainObject> creator)
            {
                return CreateAsync(command, creator);
            }

            public Task UpdateTestAsync(IAggregateCommand command, Action<TestDomainObject> updater)
            {
                return UpdateAsync(command, updater);
            }
        }

        public CommandsHandlerTests()
        {
            sut = new TestCommandHandler(domainObjectFactory.Object, domainObjectRepository.Object);
        }

        [Fact]
        public void Should_provide_factory()
        {
            Assert.Equal(domainObjectFactory.Object, sut.TestFactory);
        }

        [Fact]
        public void Should_provide_repository()
        {
            Assert.Equal(domainObjectRepository.Object, sut.TestRepository);
        }

        [Fact]
        public async Task Should_retrieve_from_repository_and_update()
        {
            var command = new TestCommand { AggregateId = id };

            var domainObject = new TestDomainObject(id, 123);

            domainObjectRepository.Setup(x => x.GetByIdAsync<TestDomainObject>(id, int.MaxValue)).Returns(Task.FromResult(domainObject)).Verifiable();
            domainObjectRepository.Setup(x => x.SaveAsync(domainObject, It.IsAny<Guid>())).Returns(Task.FromResult(true)).Verifiable();

            var isCalled = false;

            await sut.UpdateTestAsync(command, x => isCalled = true);

            domainObjectRepository.VerifyAll();

            Assert.True(isCalled);
        }

        [Fact]
        public async Task Should_create_with_factory_and_update()
        {
            var command = new TestCommand { AggregateId = id };

            var domainObject = new TestDomainObject(id, 123);

            domainObjectFactory.Setup(x => x.CreateNew(typeof(TestDomainObject), id)).Returns(domainObject).Verifiable();
            domainObjectRepository.Setup(x => x.SaveAsync(domainObject, It.IsAny<Guid>())).Returns(Task.FromResult(true)).Verifiable();

            var isCalled = false;

            await sut.CreateTestAsync(command, x => isCalled = true);

            domainObjectFactory.VerifyAll();
            domainObjectRepository.VerifyAll();

            Assert.True(isCalled);
        }
    }
}
