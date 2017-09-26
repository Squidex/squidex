// ==========================================================================
//  SchemaEventTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Contents.Old;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentEventTests
    {
        private readonly RefToken actor = new RefToken("User", Guid.NewGuid().ToString());
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema");
        private readonly Guid contentId = Guid.NewGuid();

        [Fact]
        public void Should_migrate_content_published_to_content_status_changed()
        {
            var source = CreateEvent(new ContentPublished());

            source.Migrate().ShouldBeSameEvent(CreateEvent(new ContentStatusChanged { Status = Status.Published }));
        }

        [Fact]
        public void Should_migrate_content_unpublished_to_content_status_changed()
        {
            var source = CreateEvent(new ContentUnpublished());

            source.Migrate().ShouldBeSameEvent(CreateEvent(new ContentStatusChanged { Status = Status.Draft }));
        }

        [Fact]
        public void Should_migrate_content_restored_to_content_status_changed()
        {
            var source = CreateEvent(new ContentRestored());

            source.Migrate().ShouldBeSameEvent(CreateEvent(new ContentStatusChanged { Status = Status.Draft }));
        }

        [Fact]
        public void Should_migrate_content_archived_to_content_status_changed()
        {
            var source = CreateEvent(new ContentArchived());

            source.Migrate().ShouldBeSameEvent(CreateEvent(new ContentStatusChanged { Status = Status.Archived }));
        }

        private T CreateEvent<T>(T contentEvent) where T : ContentEvent
        {
            contentEvent.Actor = actor;
            contentEvent.AppId = appId;
            contentEvent.SchemaId = schemaId;
            contentEvent.ContentId = contentId;

            return contentEvent;
        }
    }
}
