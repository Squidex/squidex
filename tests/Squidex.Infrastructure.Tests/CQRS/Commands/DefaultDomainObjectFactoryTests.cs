// ==========================================================================
//  DefaultDomainObjectFactoryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Moq;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class DefaultDomainObjectFactoryTests
    {
        private sealed class DO : DomainObject
        {
            public DO(Guid id, int version) : base(id, version)
            {
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        [Fact]
        public void Should_create_domain_object_with_autofac()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            var factoryFunction = new DomainObjectFactoryFunction<DO>(passedId =>
            {
                return new DO(passedId, 0);
            });

            serviceProvider.Setup(x => x.GetService(typeof(DomainObjectFactoryFunction<DO>))).Returns(factoryFunction);

            var sut = new DefaultDomainObjectFactory(serviceProvider.Object);

            var id = Guid.NewGuid();

            var domainObject = sut.CreateNew(typeof(DO), id);

            Assert.Equal(id, domainObject.Id);
            Assert.Equal(0, domainObject.Version);
        }
    }
}
