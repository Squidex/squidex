// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public class TemplatesTests
    {
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();

        public static readonly IEnumerable<object[]> TemplateTests = new[]
        {
            new object[] { new CreateBlog() },
            new object[] { new CreateProfile() }
        };

        [Theory]
        [MemberData(nameof(TemplateTests))]
        public async Task Should_create_schemas(ITemplate template)
        {
            var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

            var command = new CreateApp { AppId = appId.Id, Name = appId.Name, Template = template.Name };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            var sut = new TemplateCommandMiddleware(Enumerable.Repeat(template, 1));

            await sut.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<CreateSchema>.That.Matches(x => x.AppId == appId)))
                .MustHaveHappened();
        }
    }
}
