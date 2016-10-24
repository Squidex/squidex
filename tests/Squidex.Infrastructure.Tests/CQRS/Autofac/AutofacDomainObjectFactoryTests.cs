// ==========================================================================
//  AutofacDomainObjectFactoryTests.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using PinkParrot.Infrastructure.CQRS.Events;
using Xunit;

namespace PinkParrot.Infrastructure.CQRS.Autofac
{
    public class AutofacDomainObjectFactoryTests
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
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<DO>()
                .AsSelf();

            var factory = new AutofacDomainObjectFactory(containerBuilder.Build());

            var id = Guid.NewGuid();

            var domainObject = factory.CreateNew(typeof(DO), id);

            Assert.Equal(id, domainObject.Id);
            Assert.Equal(0, domainObject.Version);
        }
    }
}
