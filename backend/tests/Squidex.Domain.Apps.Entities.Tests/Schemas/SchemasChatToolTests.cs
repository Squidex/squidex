// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.AI;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class SchemasChatToolTests : GivenContext
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly SchemasChatTool sut;

    public SchemasChatToolTests()
    {
        sut = new SchemasChatTool(AppProvider, TestUtils.DefaultSerializer, urlGenerator);
    }

    [Fact]
    public async Task Should_return_schemas_if_user_has_permission()
    {
        var chatContext = new AppChatContext
        {
            BaseContext = CreateContext(PermissionIds.ForApp(PermissionIds.AppSchemasRead, App.Name).Id)
        };

        var tool = await sut.GetToolsAsync(chatContext, CancellationToken).FirstOrDefaultAsync(CancellationToken);

        Assert.NotNull(tool);
        Assert.Equal("schemas", tool.Spec.Name);
        Assert.Equal("Schemas", tool.Spec.DisplayName);

        var result = await tool.ExecuteAsync(Activator.CreateInstance<ToolContext>(), CancellationToken);

        Assert.Contains(Schema.Name, result, StringComparison.Ordinal);

        A.CallTo(() => urlGenerator.SchemasUI(AppId))
            .MustHaveHappened();

        A.CallTo(() => urlGenerator.SchemaUI(AppId, SchemaId))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_return_tools_if_user_no_permission()
    {
        var chatContext = new AppChatContext
        {
            BaseContext = FrontendContext
        };

        var tool = await sut.GetToolsAsync(chatContext, CancellationToken).FirstOrDefaultAsync(CancellationToken);

        Assert.Null(tool);
    }
}
