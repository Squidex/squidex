// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Teams.Indexes;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities;

public class AppProviderTests : GivenContext
{
    private readonly IAppsIndex indexForApps = A.Fake<IAppsIndex>();
    private readonly IRulesIndex indexForRules = A.Fake<IRulesIndex>();
    private readonly ISchemasIndex indexForSchemas = A.Fake<ISchemasIndex>();
    private readonly ITeamsIndex indexForTeams = A.Fake<ITeamsIndex>();
    private readonly AppProvider sut;

    public AppProviderTests()
    {
        sut = new AppProvider(indexForApps, indexForRules, indexForSchemas, indexForTeams, new AsyncLocalCache());
    }

    [Fact]
    public async Task Should_get_app_with_schema_from_index()
    {
        A.CallTo(() => indexForApps.GetAppAsync(AppId.Id, false, CancellationToken))
            .Returns(App);

        A.CallTo(() => indexForSchemas.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken))
            .Returns(Schema);

        var actual = await sut.GetAppWithSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken);

        Assert.Equal(Schema, actual.Item2);
    }

    [Fact]
    public async Task Should_get_team_apps_from_index()
    {
        A.CallTo(() => indexForApps.GetAppsForTeamAsync(TeamId, CancellationToken))
            .Returns([App]);

        var actual = await sut.GetTeamAppsAsync(TeamId, CancellationToken);

        Assert.Equal(App, actual.Single());
    }

    [Fact]
    public async Task Should_get_apps_from_index()
    {
        var permissions = new PermissionSet("*");

        A.CallTo(() => indexForApps.GetAppsForUserAsync("user1", permissions, CancellationToken))
            .Returns([App]);

        var actual = await sut.GetUserAppsAsync("user1", permissions, CancellationToken);

        Assert.Equal(App, actual.Single());
    }

    [Fact]
    public async Task Should_get_app_from_index()
    {
        A.CallTo(() => indexForApps.GetAppAsync(AppId.Id, false, CancellationToken))
            .Returns(App);

        var actual = await sut.GetAppAsync(AppId.Id, false, CancellationToken);

        Assert.Equal(App, actual);
    }

    [Fact]
    public async Task Should_get_app_by_name_from_index()
    {
        A.CallTo(() => indexForApps.GetAppAsync(AppId.Name, false, CancellationToken))
            .Returns(App);

        var actual = await sut.GetAppAsync(AppId.Name, false, CancellationToken);

        Assert.Equal(App, actual);
    }

    [Fact]
    public async Task Should_get_team_from_index()
    {
        A.CallTo(() => indexForTeams.GetTeamAsync(TeamId, CancellationToken))
            .Returns(Team);

        var actual = await sut.GetTeamAsync(TeamId, CancellationToken);

        Assert.Equal(Team, actual);
    }

    [Fact]
    public async Task Should_get_team_by_domain_from_index()
    {
        A.CallTo(() => indexForTeams.GetTeamByAuthDomainAsync("squidex.io", CancellationToken))
            .Returns(Team);

        var actual = await sut.GetTeamByAuthDomainAsync("squidex.io", CancellationToken);

        Assert.Equal(Team, actual);
    }

    [Fact]
    public async Task Should_get_teams_from_index()
    {
        A.CallTo(() => indexForTeams.GetTeamsAsync("user1", CancellationToken))
            .Returns([Team]);

        var actual = await sut.GetUserTeamsAsync("user1", CancellationToken);

        Assert.Equal(Team, actual.Single());
    }

    [Fact]
    public async Task Should_get_schema_from_index()
    {
        A.CallTo(() => indexForSchemas.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken))
            .Returns(Schema);

        var actual = await sut.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken);

        Assert.Equal(Schema, actual);
    }

    [Fact]
    public async Task Should_get_schema_by_name_from_index()
    {
        A.CallTo(() => indexForSchemas.GetSchemaAsync(AppId.Id, SchemaId.Name, false, CancellationToken))
            .Returns(Schema);

        var actual = await sut.GetSchemaAsync(AppId.Id, SchemaId.Name, false, CancellationToken);

        Assert.Equal(Schema, actual);
    }

    [Fact]
    public async Task Should_get_schemas_from_index()
    {
        A.CallTo(() => indexForSchemas.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns([Schema]);

        var actual = await sut.GetSchemasAsync(AppId.Id, CancellationToken);

        Assert.Equal(Schema, actual.Single());
    }

    [Fact]
    public async Task Should_get_rules_from_index()
    {
        var rule = CreateRule();

        A.CallTo(() => indexForRules.GetRulesAsync(AppId.Id, CancellationToken))
            .Returns([rule]);

        var actual = await sut.GetRulesAsync(AppId.Id, CancellationToken);

        Assert.Equal(rule, actual.Single());
    }

    [Fact]
    public async Task Should_get_rule_from_index()
    {
        var rule = CreateRule();

        A.CallTo(() => indexForRules.GetRulesAsync(AppId.Id, CancellationToken))
            .Returns([rule]);

        var actual = await sut.GetRuleAsync(AppId.Id, rule.Id, CancellationToken);

        Assert.Equal(rule, actual);
    }
}
