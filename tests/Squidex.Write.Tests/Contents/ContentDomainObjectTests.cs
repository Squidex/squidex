// ==========================================================================
//  ContentDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Contents.Commands;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Contents
{
    [Collection("Content")]
    public class ContentDomainObjectTests
    {
        private readonly Guid appId = Guid.NewGuid();
        private readonly ContentDomainObject sut;
        private readonly JObject data = new JObject();

        public ContentDomainObjectTests()
        {
            sut = new ContentDomainObject(Guid.NewGuid(), 0);
        }

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateContent { Data = data });

            Assert.Throws<DomainException>(() => sut.Create(new CreateContent { Data = data }));
        }

        [Fact]
        public void Create_should_throw_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() => sut.Create(new CreateContent()));
        }

        [Fact]
        public void Create_should_create_events()
        {
            sut.Create(new CreateContent { Data = data, AppId = appId });

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new ContentCreated { Data = data }
                    });
        }

        [Fact]
        public void Update_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Update(new UpdateContent { Data = data }));
        }

        [Fact]
        public void Update_should_throw_if_schema_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<ValidationException>(() => sut.Update(new UpdateContent()));
        }

        [Fact]
        public void Update_should_throw_if_command_is_not_valid()
        {
            CreateContent();

            Assert.Throws<ValidationException>(() => sut.Update(new UpdateContent()));
        }

        [Fact]
        public void Update_should_create_events()
        {
            CreateContent();

            sut.Update(new UpdateContent { Data = data });

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new ContentUpdated { Data = data }
                    });
        }

        [Fact]
        public void Delete_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() => sut.Delete(new DeleteContent()));
        }

        [Fact]
        public void Delete_should_throw_if_already_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() => sut.Delete(new DeleteContent()));
        }

        [Fact]
        public void Delete_should_update_properties_create_events()
        {
            CreateContent();

            sut.Delete(new DeleteContent());

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents().Select(x => x.Payload).ToArray()
                .ShouldBeEquivalentTo(
                    new IEvent[]
                    {
                        new ContentDeleted()
                    });
        }

        private void CreateContent()
        {
            sut.Create(new CreateContent { Data = data, AppId = appId });

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteContent()
        {
            sut.Delete(new DeleteContent());

            ((IAggregate)sut).ClearUncommittedEvents();
        }
    }
}
