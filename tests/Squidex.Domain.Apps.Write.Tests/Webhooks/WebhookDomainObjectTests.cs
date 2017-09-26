// ==========================================================================
//  WebhookDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Webhooks;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Xunit;

namespace Squidex.Domain.Apps.Write.Webhooks
{
    public class WebhookDomainObjectTests : HandlerTestBase<WebhookDomainObject>
    {
        private readonly Uri url = new Uri("http://squidex.io");
        private readonly WebhookDomainObject sut;

        public Guid WebhookId { get; } = Guid.NewGuid();

        public WebhookDomainObjectTests()
        {
            sut = new WebhookDomainObject(WebhookId, 0);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(new CreateWebhook { Url = url });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateWebhookCommand(new CreateWebhook { Url = url }));
            });
        }

        [Fact]
        public void Create_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.Create(CreateWebhookCommand(new CreateWebhook { Url = new Uri("/invalid", UriKind.Relative) }));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            var command = new CreateWebhook { Url = url };

            sut.Create(CreateWebhookCommand(command));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateWebhookEvent(new WebhookCreated
                    {
                        Url = url,
                        Schemas = command.Schemas,
                        SharedSecret = command.SharedSecret,
                        WebhookId = command.WebhookId
                    })
                );
        }

        [Fact]
        public void Update_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateWebhookCommand(new UpdateWebhook { Url = url }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_webhook_is_deleted()
        {
            CreateWebhook();
            DeleteWebhook();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateWebhookCommand(new UpdateWebhook { Url = url }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_command_is_not_valid()
        {
            CreateWebhook();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Update(CreateWebhookCommand(new UpdateWebhook { Url = new Uri("/invalid", UriKind.Relative) }));
            });
        }

        [Fact]
        public void Update_should_create_events()
        {
            CreateWebhook();

            var command = new UpdateWebhook { Url = url };

            sut.Update(CreateWebhookCommand(command));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateWebhookEvent(new WebhookUpdated { Url = url, Schemas = command.Schemas })
                );
        }

        [Fact]
        public void Delete_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateWebhookCommand(new DeleteWebhook()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_already_deleted()
        {
            CreateWebhook();
            DeleteWebhook();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateWebhookCommand(new DeleteWebhook()));
            });
        }

        [Fact]
        public void Delete_should_update_properties_create_events()
        {
            CreateWebhook();

            sut.Delete(CreateWebhookCommand(new DeleteWebhook()));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateWebhookEvent(new WebhookDeleted())
                );
        }

        private void CreateWebhook()
        {
            sut.Create(CreateWebhookCommand(new CreateWebhook { Url = url }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteWebhook()
        {
            sut.Delete(CreateWebhookCommand(new DeleteWebhook()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        protected T CreateWebhookEvent<T>(T @event) where T : WebhookEvent
        {
            @event.WebhookId = WebhookId;

            return CreateEvent(@event);
        }

        protected T CreateWebhookCommand<T>(T command) where T : WebhookAggregateCommand
        {
            command.WebhookId = WebhookId;

            return CreateCommand(command);
        }
    }
}
