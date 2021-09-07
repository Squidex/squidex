// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithContentIdCommandMiddlewareTests
    {
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly EnrichWithContentIdCommandMiddleware sut;

        public EnrichWithContentIdCommandMiddlewareTests()
        {
            sut = new EnrichWithContentIdCommandMiddleware();
        }

        [Fact]
        public async Task Should_replace_content_id_with_schema_id_if_placeholder_used()
        {
            var command = new UpdateContent
            {
                ContentId = DomainId.Create("_schemaId_")
            };

            await HandleAsync(command);

            Assert.Equal(schemaId.Id, command.ContentId);
        }

        [Fact]
        public async Task Should_not_replace_content_id_with_schema_for_create_command()
        {
            var command = new CreateContent
            {
                ContentId = DomainId.Create("_schemaId_")
            };

            await HandleAsync(command);

            Assert.NotEqual(schemaId.Id, command.ContentId);
        }

        [Fact]
        public async Task Should_not_replace_content_id_with_schema_id_if_placeholder_not_used()
        {
            var command = new UpdateContent
            {
                ContentId = DomainId.Create("{custom}")
            };

            await HandleAsync(command);

            Assert.NotEqual(schemaId.Id, command.ContentId);
        }

        private async Task<CommandContext> HandleAsync(ContentCommand command)
        {
            command.AppId = appId;
            command.SchemaId = schemaId;

            var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

            await sut.HandleAsync(commandContext);

            return commandContext;
        }
    }
}
