// ==========================================================================
//  WebhookCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Webhooks;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Xunit;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Webhooks
{
    public class WebhookCommandMiddlewareTests : HandlerTestBase<WebhookDomainObject>
    {
        private readonly ISchemaProvider schemaProvider = A.Fake<ISchemaProvider>();
        private readonly WebhookCommandMiddleware sut;
        private readonly WebhookDomainObject webhook;
        private readonly Uri url = new Uri("http://squidex.io");
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid webhookId = Guid.NewGuid();
        private readonly List<WebhookSchema> schemas;

        public WebhookCommandMiddlewareTests()
        {
            webhook = new WebhookDomainObject(webhookId, -1);

            schemas = new List<WebhookSchema>
            {
                new WebhookSchema { SchemaId = schemaId }
            };

            sut = new WebhookCommandMiddleware(Handler, schemaProvider);
        }

        [Fact]
        public async Task Create_should_create_webhook()
        {
            var context = CreateContextForCommand(new CreateWebhook { Schemas = schemas, Url = url, WebhookId = webhookId });

            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(schemaId, false)).Returns(Task.FromResult(A.Fake<ISchemaEntity>()));

            await TestCreate(webhook, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(schemaId, false)).MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_throw_exception_when_schema_is_not_found()
        {
            var context = CreateContextForCommand(new CreateWebhook { Schemas = schemas, Url = url, WebhookId = webhookId });

            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(schemaId, false)).Returns(Task.FromResult<ISchemaEntity>(null));

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await TestCreate(webhook, async _ =>
                {
                    await sut.HandleAsync(context);
                });
            });
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            var context = CreateContextForCommand(new UpdateWebhook { Schemas = schemas, Url = url, WebhookId = webhookId });

            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(schemaId, false)).Returns(Task.FromResult(A.Fake<ISchemaEntity>()));

            CreateWebhook();

            await TestUpdate(webhook, async _ =>
            {
                await sut.HandleAsync(context);
            });

            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(schemaId, false)).MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_exception_when_schema_is_not_found()
        {
            var context = CreateContextForCommand(new UpdateWebhook { Schemas = schemas, Url = url, WebhookId = webhookId });

            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(schemaId, false)).Returns(Task.FromResult<ISchemaEntity>(null));

            CreateWebhook();

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await TestCreate(webhook, async _ =>
                {
                    await sut.HandleAsync(context);
                });
            });
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateWebhook();

            var command = CreateContextForCommand(new DeleteWebhook { WebhookId = webhookId });

            await TestUpdate(webhook, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        private void CreateWebhook()
        {
            webhook.Create(new CreateWebhook { Url = url });
        }
    }
}
