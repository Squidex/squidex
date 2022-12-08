// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Indexes;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities;

public class AppProviderTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IAppsIndex indexForApps = A.Fake<IAppsIndex>();
    private readonly IRulesIndex indexForRules = A.Fake<IRulesIndex>();
    private readonly ISchemasIndex indexForSchemas = A.Fake<ISchemasIndex>();
    private readonly ITeamsIndex indexForTeams = A.Fake<ITeamsIndex>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly IAppEntity app;
    private readonly AppProvider sut;

    public AppProviderTests()
    {
        ct = cts.Token;

        app = Mocks.App(appId);

        sut = new AppProvider(indexForApps, indexForRules, indexForSchemas, indexForTeams, new AsyncLocalCache());
    }

    [Fact]
    public async Task Should_get_app_with_schema_from_index()
    {
        var schema = Mocks.Schema(app.NamedId(), schemaId);

        A.CallTo(() => indexForApps.GetAppAsync(app.Id, false, ct))
            .Returns(app);

        A.CallTo(() => indexForSchemas.GetSchemaAsync(app.Id, schema.Id, false, ct))
            .Returns(schema);

        var actual = await sut.GetAppWithSchemaAsync(app.Id, schemaId.Id, false, ct);

        Assert.Equal(schema, actual.Item2);
    }

    [Fact]
    public async Task Should_get_team_apps_from_index()
    {
        var team = Mocks.Team(DomainId.NewGuid());

        A.CallTo(() => indexForApps.GetAppsForTeamAsync(team.Id, ct))
            .Returns(new List<IAppEntity> { app });

        var actual = await sut.GetTeamAppsAsync(team.Id, ct);

        Assert.Equal(app, actual.Single());
    }

    [Fact]
    public async Task Should_get_apps_from_index()
    {
        var permissions = new PermissionSet("*");

        A.CallTo(() => indexForApps.GetAppsForUserAsync("user1", permissions, ct))
            .Returns(new List<IAppEntity> { app });

        var actual = await sut.GetUserAppsAsync("user1", permissions, ct);

        Assert.Equal(app, actual.Single());
    }

    [Fact]
    public async Task Should_get_app_from_index()
    {
        A.CallTo(() => indexForApps.GetAppAsync(app.Id, false, ct))
            .Returns(app);

        var actual = await sut.GetAppAsync(app.Id, false, ct);

        Assert.Equal(app, actual);
    }

    [Fact]
    public async Task Should_get_app_by_name_from_index()
    {
        A.CallTo(() => indexForApps.GetAppAsync(app.Name, false, ct))
            .Returns(app);

        var actual = await sut.GetAppAsync(app.Name, false, ct);

        Assert.Equal(app, actual);
    }

    [Fact]
    public async Task Should_get_team_from_index()
    {
        var team = Mocks.Team(DomainId.NewGuid());

        A.CallTo(() => indexForTeams.GetTeamAsync(team.Id, ct))
            .Returns(team);

        var actual = await sut.GetTeamAsync(team.Id, ct);

        Assert.Equal(team, actual);
    }

    [Fact]
    public async Task Should_get_teams_from_index()
    {
        var team = Mocks.Team(DomainId.NewGuid());

        A.CallTo(() => indexForTeams.GetTeamsAsync("user1", ct))
            .Returns(new List<ITeamEntity> { team });

        var actual = await sut.GetUserTeamsAsync("user1", ct);

        Assert.Equal(team, actual.Single());
    }

    [Fact]
    public async Task Should_get_schema_from_index()
    {
        var schema = Mocks.Schema(app.NamedId(), schemaId);

        A.CallTo(() => indexForSchemas.GetSchemaAsync(app.Id, schema.Id, false, ct))
            .Returns(schema);

        var actual = await sut.GetSchemaAsync(app.Id, schema.Id, false, ct);

        Assert.Equal(schema, actual);
    }

    [Fact]
    public async Task Should_get_schema_by_name_from_index()
    {
        var schema = Mocks.Schema(app.NamedId(), schemaId);

        A.CallTo(() => indexForSchemas.GetSchemaAsync(app.Id, schemaId.Name, false, ct))
            .Returns(schema);

        var actual = await sut.GetSchemaAsync(app.Id, schemaId.Name, false, ct);

        Assert.Equal(schema, actual);
    }

    [Fact]
    public async Task Should_get_schemas_from_index()
    {
        var schema = Mocks.Schema(app.NamedId(), schemaId);

        A.CallTo(() => indexForSchemas.GetSchemasAsync(app.Id, ct))
            .Returns(new List<ISchemaEntity> { schema });

        var actual = await sut.GetSchemasAsync(app.Id, ct);

        Assert.Equal(schema, actual.Single());
    }

    [Fact]
    public async Task Should_get_rules_from_index()
    {
        var rule = new RuleEntity();

        A.CallTo(() => indexForRules.GetRulesAsync(app.Id, ct))
            .Returns(new List<IRuleEntity> { rule });

        var actual = await sut.GetRulesAsync(app.Id, ct);

        Assert.Equal(rule, actual.Single());
    }

    [Fact]
    public async Task Should_get_rule_from_index()
    {
        var rule = new RuleEntity { Id = DomainId.NewGuid() };

        A.CallTo(() => indexForRules.GetRulesAsync(app.Id, ct))
            .Returns(new List<IRuleEntity> { rule });

        var actual = await sut.GetRuleAsync(app.Id, rule.Id, ct);

        Assert.Equal(rule, actual);
    }
}
