// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppSettingsSearchSourceTests : GivenContext
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly AppSettingsSearchSource sut;

    public AppSettingsSearchSourceTests()
    {
        sut = new AppSettingsSearchSource(urlGenerator);
    }

    [Fact]
    public async Task Should_return_empty_if_nothing_matching()
    {
        var actual = await sut.SearchAsync("xyz", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_dashboard_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppUsage);

        A.CallTo(() => urlGenerator.DashboardUI(AppId))
            .Returns("dashboard-url");

        var actual = await sut.SearchAsync("dashboard", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Dashboard", SearchResultType.Dashboard, "dashboard-url"));
    }

    [Fact]
    public async Task Should_not_return_dashboard_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("assets", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_languages_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppLanguagesRead);

        A.CallTo(() => urlGenerator.LanguagesUI(AppId))
            .Returns("languages-url");

        var actual = await sut.SearchAsync("languages", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Languages", SearchResultType.Setting, "languages-url"));
    }

    [Fact]
    public async Task Should_not_return_languages_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("assets", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_schemas_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppSchemasRead);

        A.CallTo(() => urlGenerator.SchemasUI(AppId))
            .Returns("schemas-url");

        var actual = await sut.SearchAsync("schemas", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Schemas", SearchResultType.Schema, "schemas-url"));
    }

    [Fact]
    public async Task Should_not_return_schemas_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("schemas", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_assets_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppAssetsRead);

        A.CallTo(() => urlGenerator.AssetsUI(AppId, A<string?>._))
            .Returns("assets-url");

        var actual = await sut.SearchAsync("assets", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Assets", SearchResultType.Asset, "assets-url"));
    }

    [Fact]
    public async Task Should_not_return_assets_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("assets", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_backups_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppJobsRead);

        A.CallTo(() => urlGenerator.JobsUI(AppId))
            .Returns("jobs-url");

        var actual = await sut.SearchAsync("backups", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Backups", SearchResultType.Setting, "jobs-url"));
    }

    [Fact]
    public async Task Should_return_jobs_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppJobsRead);

        A.CallTo(() => urlGenerator.JobsUI(AppId))
            .Returns("jobs-url");

        var actual = await sut.SearchAsync("jobs", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Jobs", SearchResultType.Setting, "jobs-url"));
    }

    [Fact]
    public async Task Should_not_return_backups_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("backups", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_clients_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppClientsRead);

        A.CallTo(() => urlGenerator.ClientsUI(AppId))
            .Returns("clients-url");

        var actual = await sut.SearchAsync("clients", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Clients", SearchResultType.Setting, "clients-url"));
    }

    [Fact]
    public async Task Should_not_return_clients_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("clients", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_contributors_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppContributorsRead);

        A.CallTo(() => urlGenerator.ContributorsUI(AppId))
            .Returns("contributors-url");

        var actual = await sut.SearchAsync("contributors", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Contributors", SearchResultType.Setting, "contributors-url"));
    }

    [Fact]
    public async Task Should_not_contributors_clients_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("contributors", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_subscription_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppPlansRead);

        A.CallTo(() => urlGenerator.PlansUI(AppId))
            .Returns("subscription-url");

        var actual = await sut.SearchAsync("subscription", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Subscription", SearchResultType.Setting, "subscription-url"));
    }

    [Fact]
    public async Task Should_not_subscription_clients_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("subscription", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_roles_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppRolesRead);

        A.CallTo(() => urlGenerator.RolesUI(AppId))
            .Returns("roles-url");

        var actual = await sut.SearchAsync("roles", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Roles", SearchResultType.Setting, "roles-url"));
    }

    [Fact]
    public async Task Should_not_roles_clients_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("roles", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_rules_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppRulesRead);

        A.CallTo(() => urlGenerator.RulesUI(AppId))
            .Returns("rules-url");

        var actual = await sut.SearchAsync("rules", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Rules", SearchResultType.Rule, "rules-url"));
    }

    [Fact]
    public async Task Should_not_return_rules_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("assets", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_workflows_result_if_matching_and_permission_given()
    {
        var requestContext = SetupContext(PermissionIds.AppWorkflowsRead);

        A.CallTo(() => urlGenerator.WorkflowsUI(AppId))
            .Returns("workflows-url");

        var actual = await sut.SearchAsync("workflows", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Workflows", SearchResultType.Setting, "workflows-url"));
    }

    [Fact]
    public async Task Should_not_return_workflows_result_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("workflows", ApiContext, CancellationToken);

        Assert.Empty(actual);
    }

    private Context SetupContext(string permission)
    {
        return CreateContext(false, PermissionIds.ForApp(permission, AppId.Name).Id);
    }
}
