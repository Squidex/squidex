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
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps;

public class AppChatToolsTests : GivenContext
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly AppChatTools sut;

    public AppChatToolsTests()
    {
        sut = new AppChatTools(TestUtils.DefaultSerializer, urlGenerator);
    }

    [Fact]
    public async Task Should_return_clients_if_user_has_permission()
    {
        App = App with
        {
            Clients = App.Clients.Add("default", "secret")
        };

        var chatContext = new AppChatContext
        {
            BaseContext = CreateContext(PermissionIds.ForApp(PermissionIds.AppClientsRead, App.Name).Id)
        };

        var tool = await sut.GetToolsAsync(chatContext, default).FirstOrDefaultAsync();

        Assert.NotNull(tool);
        Assert.Equal("clients", tool.Spec.Name);
        Assert.Equal("Clients", tool.Spec.DisplayName);

        var result = await tool.ExecuteAsync(Activator.CreateInstance<ToolContext>(), default);

        Assert.Contains($"{App.Name}:default", result, StringComparison.Ordinal);

        A.CallTo(() => urlGenerator.ClientsUI(AppId))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_languages_if_user_has_permission()
    {
        App = App with
        {
            Languages = App.Languages.Set(Language.DE)
        };

        var chatContext = new AppChatContext
        {
            BaseContext = CreateContext(PermissionIds.ForApp(PermissionIds.AppLanguages, App.Name).Id)
        };

        var tool = await sut.GetToolsAsync(chatContext, default).FirstOrDefaultAsync();

        Assert.NotNull(tool);
        Assert.Equal("languages", tool.Spec.Name);
        Assert.Equal("Languages", tool.Spec.DisplayName);

        var result = await tool.ExecuteAsync(Activator.CreateInstance<ToolContext>(), default);

        Assert.Contains($"\"de\"", result, StringComparison.Ordinal);

        A.CallTo(() => urlGenerator.LanguagesUI(AppId))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_roles_if_user_has_permission()
    {
        App = App with
        {
            Roles = App.Roles.Add("viewers")
        };

        var chatContext = new AppChatContext
        {
            BaseContext = CreateContext(PermissionIds.ForApp(PermissionIds.AppRoles, App.Name).Id)
        };

        var tool = await sut.GetToolsAsync(chatContext, default).FirstOrDefaultAsync();

        Assert.NotNull(tool);
        Assert.Equal("roles", tool.Spec.Name);
        Assert.Equal("Roles", tool.Spec.DisplayName);

        var result = await tool.ExecuteAsync(Activator.CreateInstance<ToolContext>(), default);

        Assert.Contains($"viewers", result, StringComparison.Ordinal);

        A.CallTo(() => urlGenerator.RolesUI(AppId))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_plan_if_user_has_permission()
    {
        App = App with
        {
            Plan = new AssignedPlan(User, "Business")
        };

        var chatContext = new AppChatContext
        {
            BaseContext = CreateContext(PermissionIds.ForApp(PermissionIds.AppPlans, App.Name).Id)
        };

        var tool = await sut.GetToolsAsync(chatContext, default).FirstOrDefaultAsync();

        Assert.NotNull(tool);
        Assert.Equal("plan", tool.Spec.Name);
        Assert.Equal("Plan", tool.Spec.DisplayName);

        var result = await tool.ExecuteAsync(Activator.CreateInstance<ToolContext>(), default);

        Assert.Contains($"Business", result, StringComparison.Ordinal);

        A.CallTo(() => urlGenerator.PlansUI(AppId))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_return_tools_if_user_no_permission()
    {
        var chatContext = new AppChatContext
        {
            BaseContext = FrontendContext
        };

        var tool = await sut.GetToolsAsync(chatContext, default).FirstOrDefaultAsync();

        Assert.Null(tool);
    }
}
