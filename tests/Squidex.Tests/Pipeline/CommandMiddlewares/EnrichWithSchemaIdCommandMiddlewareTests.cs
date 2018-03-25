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
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline.CommandMiddlewares;
using Xunit;

namespace Squidex.Tests.Pipeline.CommandMiddlewares
{
    public class EnrichWithSchemaIdCommandMiddlewareTests
    {
        private readonly Mock<IActionContextAccessor> actionContextAccessor = new Mock<IActionContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly ActionDescriptor actionDescriptor = new ActionDescriptor();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly string appName = "app";
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly string schemaName = "schema";
        private readonly CreateContent command = new CreateContent();
        private readonly RouteData routeData = new RouteData();
        private ISchemaEntity schema;

        [Fact]
        public async Task HandleAsync_should_throw_exception_if_schema_not_found()
        {
            var context = new CommandContext(command, commandBus);
            var sut = SetupSchemaCommand(false);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() =>
            {
                return sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task HandleAsync_should_find_schema_id_by_name()
        {
            var context = new CommandContext(command, commandBus);
            SetupSchema();

            var sut = SetupSchemaCommand(false);

            await sut.HandleAsync(context);

            Assert.Equal(new NamedId<Guid>(schemaId, schemaName), command.SchemaId);
        }

        [Fact]
        public async Task HandleAsync_should_find_schema_id_by_id()
        {
            var context = new CommandContext(command, commandBus);
            SetupSchema();

            var sut = SetupSchemaCommand(true);

            await sut.HandleAsync(context);

            Assert.Equal(new NamedId<Guid>(schemaId, schemaName), command.SchemaId);
        }

        private void SetupSchema()
        {
            var schemaDef = new Schema(schemaName);
            var stringValidatorProperties = new StringFieldProperties
            {
                Pattern = "A-Z"
            };
            var stringFieldWithValidator = new StringField(1, "validator", Partitioning.Invariant, stringValidatorProperties);

            schemaDef = schemaDef.AddField(stringFieldWithValidator);

            schema = new SchemaState
            {
                Name = schemaName,
                Id = schemaId,
                AppId = new NamedId<Guid>(appId, appName),
                SchemaDef = schemaDef
            };
        }

        private EnrichWithSchemaIdCommandMiddleware SetupSchemaCommand(bool byId)
        {
            command.AppId = new NamedId<Guid>(appId, appName);

            if (byId)
            {
                routeData.Values.Add("name", schemaId.ToString());
                A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false)).Returns(schema);
            }
            else
            {
                routeData.Values.Add("name", "schema");
                A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaName)).Returns(schema);
            }

            var actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor);
            actionContextAccessor.Setup(x => x.ActionContext).Returns(actionContext);
            return new EnrichWithSchemaIdCommandMiddleware(appProvider, actionContextAccessor.Object);
        }
    }
}
