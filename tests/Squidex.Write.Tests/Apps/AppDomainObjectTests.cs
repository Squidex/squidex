// ==========================================================================
//  AppDomainObjectTest.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure;
using PinkParrot.Write.Apps;
using PinkParrot.Write.Apps.Commands;
using Xunit;
using System.Linq;
using FluentAssertions;
using PinkParrot.Events.Apps;

namespace PinkParrot.Write.Tests.Apps
{
    public class AppDomainObjectTests
    {
        private const string TestName = "app";
        private readonly AppDomainObject sut = new AppDomainObject(Guid.NewGuid(), 0);

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateApp { Name = TestName });

            Assert.Throws<DomainException>(() => sut.Create(new CreateApp { Name = TestName }));
        }

        [Fact]
        public void Create_should_throw_if_command_is_invalid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateApp()));
        }

        [Fact]
        public void Create_should_specify_name()
        {
            sut.Create(new CreateApp { Name = TestName });

            Assert.Equal(TestName, sut.Name);

            sut.GetUncomittedEvents()
                .Select(x => x.Payload as AppCreated)
                .Single()
                .ShouldBeEquivalentTo(
                    new AppCreated { Name = TestName });
        }
    }
}
