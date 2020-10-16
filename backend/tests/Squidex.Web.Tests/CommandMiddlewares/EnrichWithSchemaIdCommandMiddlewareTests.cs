// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web.Pipeline;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithSchemaIdCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly EnrichWithSchemaIdCommandMiddleware sut;

        public EnrichWithSchemaIdCommandMiddlewareTests()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(appId));

            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            sut = new EnrichWithSchemaIdCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_throw_exception_if_schema_not_found()
        {
            var command = new CreateContent { AppId = appId };
            var context = Ctx(command);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_assign_schema_id_and_name_to_app_command()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(schemaId));

            var command = new CreateContent();
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.Equal(schemaId, command.SchemaId);
        }

        [Fact]
        public async Task Should_assign_schema_id_from_id()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(schemaId));

            var command = new UpdateSchema();
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.Equal(schemaId, command.SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(schemaId));

            var command = new CreateSchema { SchemaId = DomainId.NewGuid() };
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.NotEqual(schemaId.Id, command.SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id_and_name()
        {
            httpContext.Features.Set<ISchemaFeature>(new SchemaFeature(schemaId));

            var command = new CreateContent { SchemaId = NamedId.Of(DomainId.NewGuid(), "other-app") };
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.NotEqual(schemaId, command.SchemaId);
        }

        private CommandContext Ctx(ICommand command)
        {
            return new CommandContext(command, commandBus);
        }
    }
}
