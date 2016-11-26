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
        private sealed class User : DomainObject
        {
            public User(Guid id, int version)
                : base(id, version)
            {
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        private sealed class UserDomainObject : DomainObject
        {
            public UserDomainObject(Guid id, int version) 
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
            var user = new User(Guid.NewGuid(), 1);

            var name = sut.GetStreamName(typeof(User), user.Id);

            Assert.Equal($"squidex-user-{user.Id}", name);
        }

        [Fact]
        public void Should_calculate_name_and_remove_suffix()
        {
            var sut = new DefaultNameResolver("Squidex");
            var user = new UserDomainObject(Guid.NewGuid(), 1);

            var name = sut.GetStreamName(typeof(User), user.Id);

            Assert.Equal($"squidex-user-{user.Id}", name);
        }
    }
}
