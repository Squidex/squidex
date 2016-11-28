// ==========================================================================
//  DefaultNameResolverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public class DefaultNameResolverTests
    {
        private sealed class MyUser : DomainObject
        {
            public MyUser(Guid id, int version)
                : base(id, version)
            {
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        private sealed class MyUserDomainObject : DomainObject
        {
            public MyUserDomainObject(Guid id, int version) 
                : base(id, version)
            {
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        [Fact]
        public void Should_calculate_name()
        {
            var sut = new DefaultNameResolver("Squidex");
            var user = new MyUser(Guid.NewGuid(), 1);

            var name = sut.GetStreamName(typeof(MyUser), user.Id);

            Assert.Equal($"squidex-myUser-{user.Id}", name);
        }

        [Fact]
        public void Should_calculate_name_and_remove_suffix()
        {
            var sut = new DefaultNameResolver("Squidex");
            var user = new MyUserDomainObject(Guid.NewGuid(), 1);

            var name = sut.GetStreamName(typeof(MyUserDomainObject), user.Id);

            Assert.Equal($"squidex-myUser-{user.Id}", name);
        }
    }
}
