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
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public class EnrichWithSchemaIdCommandMiddlewareTests
    {
        private readonly IActionContextAccessor actionContextAccessor = A.Fake<IActionContextAccessor>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionContext actionContext = new ActionContext();
        private readonly EnrichWithSchemaIdCommandMiddleware sut;

        public EnrichWithSchemaIdCommandMiddlewareTests()
        {
            actionContext.RouteData = new RouteData();
            actionContext.HttpContext = httpContext;

            A.CallTo(() => actionContextAccessor.ActionContext)
                .Returns(actionContext);

            sut = new EnrichWithSchemaIdCommandMiddleware(appProvider, actionContextAccessor);
        }

        [Fact]
        public async Task Should_throw_exception_if_app_not_found()
        {
            SetupApp(out var appId, out _);
            SetupSchema(appId, out _, out _);

            A.CallTo(() => appProvider.GetSchemaAsync(appId, "other-schema"))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            actionContext.RouteData.Values["name"] = "other-schema";

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_do_nothing_when_context_is_null()
        {
            A.CallTo(() => actionContextAccessor.ActionContext)
                .Returns(null);

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Null(command.Actor);
        }

        [Fact]
        public async Task Should_do_nothing_when_route_has_no_parameter()
        {
            SetupApp(out var appId, out _);
            SetupSchema(appId, out _, out _);

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Null(command.Actor);
        }

        [Fact]
        public async Task Should_assign_schema_id_and_name_from_name()
        {
            SetupApp(out var appId, out _);
            SetupSchema(appId, out var schemaId, out var schemaName);

            actionContext.RouteData.Values["name"] = schemaName;

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(NamedId.Of(schemaId, schemaName), command.SchemaId);
        }

        [Fact]
        public async Task Should_assign_schema_id_and_name_from_id()
        {
            SetupApp(out var appId, out _);
            SetupSchema(appId, out var schemaId, out var schemaName);

            actionContext.RouteData.Values["name"] = schemaId.ToString();

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(NamedId.Of(schemaId, schemaName), command.SchemaId);
        }

        [Fact]
        public async Task Should_assign_schema_id_from_id()
        {
            SetupApp(out var appId, out _);
            SetupSchema(appId, out var schemaId, out _);

            actionContext.RouteData.Values["name"] = schemaId.ToString();

            var command = new UpdateSchema();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(schemaId, command.SchemaId);
        }

        [Fact]
        public async Task Should_use_app_id_from_command()
        {
            var appId = NamedId.Of(Guid.NewGuid(), "my-app");

            SetupSchema(appId.Id, out var schemaId, out var schemaName);

            actionContext.RouteData.Values["name"] = schemaId.ToString();

            var command = new CreateContent { AppId = appId };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(NamedId.Of(schemaId, schemaName), command.SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id()
        {
            SetupApp(out var appId, out _);
            SetupSchema(appId, out var schemaId, out _);

            var command = new CreateSchema { SchemaId = Guid.NewGuid() };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(schemaId, command.SchemaId);
        }

        [Fact]
        public async Task Should_not_override_schema_id_and_name()
        {
            SetupApp(out var appId, out var appName);
            SetupSchema(appId, out _, out _);

            var command = new CreateContent { SchemaId = NamedId.Of(Guid.NewGuid(), "other-schema") };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(NamedId.Of(appId, appName), command.AppId);
        }

        private void SetupSchema(Guid appId, out Guid schemaId, out string schemaName)
        {
            schemaId = Guid.NewGuid();
            schemaName = "my-schema";

            var schemaEntity = A.Fake<ISchemaEntity>();
            A.CallTo(() => schemaEntity.Id).Returns(schemaId);
            A.CallTo(() => schemaEntity.Name).Returns(schemaName);

            var temp1 = schemaName;
            var temp2 = schemaId;

            A.CallTo(() => appProvider.GetSchemaAsync(appId, temp1))
                .Returns(schemaEntity);
            A.CallTo(() => appProvider.GetSchemaAsync(appId, temp2, false))
                .Returns(schemaEntity);
        }

        private void SetupApp(out Guid appId, out string appName)
        {
            appId = Guid.NewGuid();
            appName = "my-app";

            var appEntity = A.Fake<IAppEntity>();
            A.CallTo(() => appEntity.Id).Returns(appId);
            A.CallTo(() => appEntity.Name).Returns(appName);

            httpContext.Features.Set<IAppFeature>(new AppApiFilter.AppFeature(appEntity));
        }
    }
}
