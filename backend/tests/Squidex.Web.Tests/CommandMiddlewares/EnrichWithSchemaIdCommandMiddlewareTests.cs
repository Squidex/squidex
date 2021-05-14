// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web.Pipeline;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithSchemaIdCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly EnrichWithSchemaIdCommandMiddleware sut;

        public EnrichWithSchemaIdCommandMiddlewareTests()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));

            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            sut = new EnrichWithSchemaIdCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_throw_exception_if_schema_not_found()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => HandleAsync(new CreateContent()));
        }

        [Fact]
        public async Task Should_assign_schema_id_and_name_to_app_command()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(Mocks.Schema(appId, schemaId)));

            var context = await HandleAsync(new CreateContent());

            Assert.Equal(schemaId, ((ISchemaCommand)context.Command).SchemaId);
        }

        [Fact]
        public async Task Should_assign_schema_id_from_id()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(Mocks.Schema(appId, schemaId)));

            var context = await HandleAsync(new UpdateSchema());

            Assert.Equal(schemaId, ((ISchemaCommand)context.Command).SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(Mocks.Schema(appId, schemaId)));

            var customId = DomainId.NewGuid();

            var context = await HandleAsync(new CreateSchema { SchemaId = customId });

            Assert.Equal(customId, ((CreateSchema)context.Command).SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id_and_name()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(Mocks.Schema(appId, schemaId)));

            var customId = NamedId.Of(DomainId.NewGuid(), "other-app");

            var context = await HandleAsync(new CreateContent { SchemaId = customId });

            Assert.Equal(customId, ((ISchemaCommand)context.Command).SchemaId);
        }

        private async Task<CommandContext> HandleAsync(IAppCommand command)
        {
            command.AppId = appId;

            var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

            await sut.HandleAsync(commandContext);

            return commandContext;
        }
    }
}
