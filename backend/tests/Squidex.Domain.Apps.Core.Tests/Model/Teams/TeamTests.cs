// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Teams;

#pragma warning disable SA1310 // Field names must not contain underscore

public class TeamTests
{
    private readonly Team team_0 = new Team();

    [Fact]
    public void Should_throw_exception_if_new_name_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => team_0.Rename(null!));
    }

    [Fact]
    public void Should_rename()
    {
        var newName = "MyTeam";

        var team_1 = team_0.Rename(newName);
        var team_2 = team_1.Rename(newName);

        Assert.NotSame(team_0, team_1);
        Assert.Equal(newName, team_1.Name);
        Assert.Equal(newName, team_2.Name);
        Assert.Same(team_1, team_2);
    }

    [Fact]
    public void Should_update_plan()
    {
        var plan1 = new AssignedPlan(RefToken.User("me"), "Premium");
        var plan2 = new AssignedPlan(RefToken.User("me"), "Premium");

        var team_1 = team_0.ChangePlan(plan1);
        var team_2 = team_1.ChangePlan(plan2);

        Assert.NotSame(team_0, team_1);
        Assert.Equal(plan1, team_1.Plan);
        Assert.Equal(plan2, team_2.Plan);
        Assert.Same(team_1, team_2);
    }

    [Fact]
    public void Should_update_auth_scheme()
    {
        var scheme1 = new AuthScheme { Authority = "authority1" };
        var scheme2 = new AuthScheme { Authority = "authority1" };

        var team_1 = team_0.ChangeAuthScheme(scheme1);
        var team_2 = team_1.ChangeAuthScheme(scheme2);

        Assert.NotSame(team_0, team_1);
        Assert.Equal(scheme1, team_1.AuthScheme);
        Assert.Equal(scheme1, team_2.AuthScheme);
        Assert.Same(team_1, team_2);
    }

    [Fact]
    public void Should_update_contributors()
    {
        var team_1 = team_0.UpdateContributors(true, (_, c) => c.Assign("me", Role.Owner));
        var team_2 = team_1.UpdateContributors(true, (_, c) => c.Assign("me", Role.Owner));

        Assert.NotSame(team_0, team_1);
        Assert.Single(team_1.Contributors);
        Assert.Single(team_2.Contributors);
        Assert.Same(team_1, team_2);
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Teams/Team.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<Team>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Teams/Team.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<Team>(json));

        Assert.Equal(json, serialized);
    }
}
