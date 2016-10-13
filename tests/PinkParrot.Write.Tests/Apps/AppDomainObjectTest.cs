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

namespace PinkParrot.Write.Tests.Apps
{
    public class AppDomainObjectTest
    {
        private readonly AppDomainObject sut = new AppDomainObject(Guid.NewGuid(), 0);

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateApp { Name = "app" });

            Assert.Throws<DomainException>(() => sut.Create(new CreateApp { Name = "app" }));
        }

        [Fact]
        public void Create_should_specify_name()
        {
            sut.Create(new CreateApp { Name = "app" });

            Assert.Equal("app", sut.Name);
        }
    }
}
