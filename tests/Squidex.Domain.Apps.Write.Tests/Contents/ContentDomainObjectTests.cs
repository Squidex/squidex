// ==========================================================================
//  ContentDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentDomainObjectTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentDomainObject sut;
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

        public Guid ContentId { get; } = Guid.NewGuid();

        public ContentDomainObjectTests()
        {
            sut = new ContentDomainObject(ContentId, 0);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(new CreateContent { Data = data });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateContentCommand(new CreateContent { Data = data }));
            });
        }

        [Fact]
        public void Create_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.Create(CreateContentCommand(new CreateContent()));
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
                    CreateContentEvent(new ContentPublished())
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

            Assert.Throws<ValidationException>(() =>
            {
                sut.Update(CreateContentCommand(new UpdateContent()));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_command_is_not_valid()
        {
            CreateContent();

            Assert.Throws<ValidationException>(() =>
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

            Assert.Throws<ValidationException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent()));
            });
        }

        [Fact]
        public void Patch_should_throw_exception_if_command_is_not_valid()
        {
            CreateContent();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent()));
            });
        }

        [Fact]
        public void Patch_should_create_events()
        {
            CreateContent();

            sut.Patch(CreateContentCommand(new PatchContent { Data = otherData }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
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
        public void Publish_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateContentCommand(new PublishContent()));
            });
        }

        [Fact]
        public void Publish_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateContentCommand(new PublishContent()));
            });
        }

        [Fact]
        public void Publish_should_refresh_properties_and_create_events()
        {
            CreateContent();

            sut.Publish(CreateContentCommand(new PublishContent()));

            Assert.True(sut.IsPublished);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentPublished())
                );
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateContentCommand(new UnpublishContent()));
            });
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateContentCommand(new UnpublishContent()));
            });
        }

        [Fact]
        public void Unpublish_should_refresh_properties_and_create_events()
        {
            CreateContent();
            PublishContent();

            sut.Unpublish(CreateContentCommand(new UnpublishContent()));

            Assert.False(sut.IsPublished);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUnpublished())
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
        public void Delete_should_update_properties_create_events()
        {
            CreateContent();

            sut.Delete(CreateContentCommand(new DeleteContent()));

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );
        }

        private void CreateContent()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void UpdateContent()
        {
            sut.Update(CreateContentCommand(new UpdateContent { Data = data }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void PublishContent()
        {
            sut.Publish(CreateContentCommand(new PublishContent()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteContent()
        {
            sut.Delete(CreateContentCommand(new DeleteContent()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        protected T CreateContentEvent<T>(T @event) where T : ContentEvent
        {
            @event.ContentId = ContentId;

            return CreateEvent(@event);
        }

        protected T CreateContentCommand<T>(T command) where T : ContentCommand
        {
            command.ContentId = ContentId;

            return CreateCommand(command);
        }
    }
}
