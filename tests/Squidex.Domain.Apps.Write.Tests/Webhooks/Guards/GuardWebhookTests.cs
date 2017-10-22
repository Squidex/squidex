// ==========================================================================
//  GuardWebhookTests.cs
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
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Webhooks.Guards
{
    public class GuardWebhookTests
    {
        private readonly ISchemaProvider schemas = A.Fake<ISchemaProvider>();

        public GuardWebhookTests()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_url_defined()
        {
            var command = new CreateWebhook();

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanCreate(command, schemas));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_url_not_valid()
        {
            var command = new CreateWebhook { Url = new Uri("/invalid", UriKind.Relative) };

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanCreate(command, schemas));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_schema_id_not_found()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(A<Guid>.Ignored, false))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            var command = new CreateWebhook
            {
                Schemas = new List<WebhookSchema>
                {
                    new WebhookSchema()
                },
                Url = new Uri("/invalid", UriKind.Relative)
            };

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanCreate(command, schemas));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_schema_id_found()
        {
            var command = new CreateWebhook
            {
                Schemas = new List<WebhookSchema>
                {
                    new WebhookSchema()
                },
                Url = new Uri("/invalid", UriKind.Relative)
            };

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanCreate(command, schemas));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_url_not_defined()
        {
            var command = new UpdateWebhook();

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanUpdate(command, schemas));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_url_not_valid()
        {
            var command = new UpdateWebhook { Url = new Uri("/invalid", UriKind.Relative) };

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanUpdate(command, schemas));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_schema_id_not_found()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(A<Guid>.Ignored, false))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            var command = new UpdateWebhook
            {
                Schemas = new List<WebhookSchema>
                {
                    new WebhookSchema()
                },
                Url = new Uri("/invalid", UriKind.Relative)
            };

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanUpdate(command, schemas));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_schema_id_found()
        {
            var command = new UpdateWebhook
            {
                Schemas = new List<WebhookSchema>
                {
                    new WebhookSchema()
                },
                Url = new Uri("/invalid", UriKind.Relative)
            };

            await Assert.ThrowsAsync<ValidationException>(() => GuardWebhook.CanUpdate(command, schemas));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteWebhook();

            GuardWebhook.CanDelete(command);
        }
    }
}
