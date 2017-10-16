// ==========================================================================
//  AppEventTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Apps.Old;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppEventTests
    {
        private readonly RefToken actor = new RefToken("User", Guid.NewGuid().ToString());
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        [Fact]
        public void Should_migrate_client_changed_as_reader_to_client_updated()
        {
            var source = CreateEvent(new AppClientChanged { IsReader = true });

            source.Migrate().ShouldBeSameEvent(CreateEvent(new AppClientUpdated { Permission = AppClientPermission.Reader }));
        }

        [Fact]
        public void Should_migrate_client_changed_as_writer_to_client_updated()
        {
            var source = CreateEvent(new AppClientChanged { IsReader = false });

            source.Migrate().ShouldBeSameEvent(CreateEvent(new AppClientUpdated { Permission = AppClientPermission.Editor }));
        }

        private T CreateEvent<T>(T contentEvent) where T : AppEvent
        {
            contentEvent.Actor = actor;
            contentEvent.AppId = appId;

            return contentEvent;
        }
    }
}
