﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentDomainObjectTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly NamedContentData data =
            new NamedContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddValue("iv", 1));
        private readonly NamedContentData otherData =
            new NamedContentData()
                .AddField("field2",
                    new ContentFieldData()
                        .AddValue("iv", 2));
        private readonly NamedContentData patched;
        private readonly Guid contentId = Guid.NewGuid();
        private readonly ContentDomainObject sut = new ContentDomainObject();

        protected override Guid Id
        {
            get { return contentId; }
        }

        public ContentDomainObjectTests()
        {
            patched = otherData.MergeInto(data);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(CreateCommand(new CreateContent { Data = data }));

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateContentCommand(new CreateContent { Data = data }));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data })
                );
        }

        [Fact]
        public void Create_should_also_publish_if_set_to_true()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data, Publish = true }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published })
                );
        }

        [Fact]
        public void Update_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateContentCommand(new UpdateContent { Data = data }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateContentCommand(new UpdateContent()));
            });
        }

        [Fact]
        public void Update_should_create_events()
        {
            CreateContent();

            sut.Update(CreateContentCommand(new UpdateContent { Data = otherData }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );
        }

        [Fact]
        public void Update_should_not_create_event_for_same_data()
        {
            CreateContent();
            UpdateContent();

            sut.Update(CreateContentCommand(new UpdateContent { Data = data }));

            sut.GetUncomittedEvents().Should().BeEmpty();
        }

        [Fact]
        public void Patch_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent { Data = data }));
            });
        }

        [Fact]
        public void Patch_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent()));
            });
        }

        [Fact]
        public void Patch_should_create_events()
        {
            CreateContent();
            UpdateContent();

            sut.Patch(CreateContentCommand(new PatchContent { Data = otherData }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = patched })
                );
        }

        [Fact]
        public void Patch_should_not_create_event_for_same_data()
        {
            CreateContent();
            UpdateContent();

            sut.Patch(CreateContentCommand(new PatchContent { Data = data }));

            sut.GetUncomittedEvents().Should().BeEmpty();
        }

        [Fact]
        public void ChangeStatus_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.ChangeStatus(CreateContentCommand(new ChangeContentStatus()));
            });
        }

        [Fact]
        public void ChangeStatus_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.ChangeStatus(CreateContentCommand(new ChangeContentStatus()));
            });
        }

        [Fact]
        public void ChangeStatus_should_refresh_properties_and_create_events()
        {
            CreateContent();

            sut.ChangeStatus(CreateContentCommand(new ChangeContentStatus { Status = Status.Published }));

            Assert.Equal(Status.Published, sut.Snapshot.Status);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published })
                );
        }

        [Fact]
        public void Delete_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateContentCommand(new DeleteContent()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_already_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateContentCommand(new DeleteContent()));
            });
        }

        [Fact]
        public void Delete_should_update_properties_and_create_events()
        {
            CreateContent();

            sut.Delete(CreateContentCommand(new DeleteContent()));

            Assert.True(sut.Snapshot.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );
        }

        private void CreateContent()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data }));
            sut.ClearUncommittedEvents();
        }

        private void UpdateContent()
        {
            sut.Update(CreateContentCommand(new UpdateContent { Data = data }));
            sut.ClearUncommittedEvents();
        }

        private void ChangeStatus(Status status)
        {
            sut.ChangeStatus(CreateContentCommand(new ChangeContentStatus { Status = status }));
            sut.ClearUncommittedEvents();
        }

        private void DeleteContent()
        {
            sut.Delete(CreateContentCommand(new DeleteContent()));
            sut.ClearUncommittedEvents();
        }

        protected T CreateContentEvent<T>(T @event) where T : ContentEvent
        {
            @event.ContentId = contentId;

            return CreateEvent(@event);
        }

        protected T CreateContentCommand<T>(T command) where T : ContentCommand
        {
            command.ContentId = contentId;

            return CreateCommand(command);
        }
    }
}
