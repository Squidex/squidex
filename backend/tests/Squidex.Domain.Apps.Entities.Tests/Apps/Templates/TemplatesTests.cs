// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
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
            new object[] { new CreateBlogCommandMiddleware(), "blog" },
            new object[] { new CreateIdentityCommandMiddleware(), "identity" },
            new object[] { new CreateProfileCommandMiddleware(), "profile" }
        };

        [Theory]
        [MemberData(nameof(TemplateTests))]
        public async Task Should_create_schemas(ICommandMiddleware middleware, string template)
        {
            var command = new CreateApp { AppId = DomainId.NewGuid(), Name = "my-app", Template = template };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await middleware.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<CreateSchema>._))
                .MustHaveHappened();
        }
    }
}
