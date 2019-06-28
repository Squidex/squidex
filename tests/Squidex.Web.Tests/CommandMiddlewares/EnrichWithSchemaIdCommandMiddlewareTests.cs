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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithSchemaIdCommandMiddlewareTests
    {
        private readonly IActionContextAccessor actionContextAccessor = A.Fake<IActionContextAccessor>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionContext actionContext = new ActionContext();
        private readonly EnrichWithSchemaIdCommandMiddleware sut;

        public EnrichWithSchemaIdCommandMiddlewareTests()
        {
            actionContext.RouteData = new RouteData();
            actionContext.HttpContext = httpContext;

            A.CallTo(() => actionContextAccessor.ActionContext)
                .Returns(actionContext);

            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Id).Returns(appId.Id);
            A.CallTo(() => app.Name).Returns(appId.Name);

            httpContext.Context().App = app;

            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.Id).Returns(schemaId.Id);
            A.CallTo(() => schema.SchemaDef).Returns(new Schema(schemaId.Name));

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schema);
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(schema);

            sut = new EnrichWithSchemaIdCommandMiddleware(appProvider, actionContextAccessor);
        }

        [Fact]
        public async Task Should_throw_exception_if_schema_not_found()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, "other-schema"))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            actionContext.RouteData.Values["name"] = "other-schema";

            var command = new CreateContent { AppId = appId };
            var context = new CommandContext(command, commandBus);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_do_nothing_when_route_has_no_parameter()
        {
            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Null(command.Actor);
        }

        [Fact]
        public async Task Should_assign_schema_id_and_name_from_name()
        {
            actionContext.RouteData.Values["name"] = schemaId.Name;

            var command = new CreateContent { AppId = appId };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(schemaId, command.SchemaId);
        }

        [Fact]
        public async Task Should_assign_schema_id_and_name_from_id()
        {
            actionContext.RouteData.Values["name"] = schemaId.Id;

            var command = new CreateContent { AppId = appId };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(schemaId, command.SchemaId);
        }

        [Fact]
        public async Task Should_assign_schema_id_from_id()
        {
            actionContext.RouteData.Values["name"] = schemaId.Name;

            var command = new UpdateSchema();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(schemaId.Id, command.SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id()
        {
            var command = new CreateSchema { SchemaId = Guid.NewGuid() };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(schemaId.Id, command.SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id_and_name()
        {
            var command = new CreateContent { SchemaId = NamedId.Of(Guid.NewGuid(), "other-schema") };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(appId, command.AppId);
        }
    }
}
