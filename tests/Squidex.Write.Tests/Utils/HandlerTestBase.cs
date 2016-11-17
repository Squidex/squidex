// ==========================================================================
//  HandlerTestBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write.Tests.Utils
{
    public abstract class HandlerTestBase<T> where T : DomainObject
    {
        private readonly Mock<IDomainObjectFactory> domainObjectFactory = new Mock<IDomainObjectFactory>();
        private readonly Mock<IDomainObjectRepository> domainObjectRepository = new Mock<IDomainObjectRepository>();
        private readonly Guid id = Guid.NewGuid();

        protected Guid Id
        {
            get { return id; }
        }

        protected Mock<IDomainObjectFactory> DomainObjectFactory
        {
            get { return domainObjectFactory; }
        }

        protected Mock<IDomainObjectRepository> DomainObjectRepository
        {
            get { return domainObjectRepository; }
        }

        public async Task TestCreate(T domainObject, Func<T, Task> action, bool succeeded = true)
        {
            domainObjectFactory.Setup(x => x.CreateNew(typeof(T), id)).Returns(domainObject).Verifiable();
            domainObjectRepository.Setup(x => x.SaveAsync(domainObject, It.IsAny<Guid>())).Returns(Task.FromResult(true)).Verifiable();

            await action(domainObject);

            if (succeeded)
            {
                domainObjectFactory.VerifyAll();
                domainObjectRepository.VerifyAll();
            }
        }

        public async Task TestUpdate(T domainObject, Func<T, Task> action, bool succeeded = true)
        {
            domainObjectRepository.Setup(x => x.GetByIdAsync<T>(domainObject.Id, int.MaxValue)).Returns(Task.FromResult(domainObject)).Verifiable();
            domainObjectRepository.Setup(x => x.SaveAsync(domainObject, It.IsAny<Guid>())).Returns(Task.FromResult(true)).Verifiable();

            await action(domainObject);

            if (succeeded)
            {
                domainObjectRepository.VerifyAll();
            }
        }
    }
}
