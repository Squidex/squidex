// ==========================================================================
//  DefaultStreamNameResolverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class DefaultStreamNameResolverTests
    {
        private readonly DefaultStreamNameResolver sut = new DefaultStreamNameResolver();

        private sealed class MyUser : DomainObjectBase
        {
            public MyUser(Guid id, int version)
                : base(id, version)
            {
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        private sealed class MyUserDomainObject : DomainObjectBase
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
            var user = new MyUser(Guid.NewGuid(), 1);

            var name = sut.GetStreamName(typeof(MyUser), user.Id);

            Assert.Equal($"myUser-{user.Id}", name);
        }

        [Fact]
        public void Should_calculate_name_and_remove_suffix()
        {
            var user = new MyUserDomainObject(Guid.NewGuid(), 1);

            var name = sut.GetStreamName(typeof(MyUserDomainObject), user.Id);

            Assert.Equal($"myUser-{user.Id}", name);
        }
    }
}
