// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Apps;

#pragma warning disable SA1310 // Field names must not contain underscore

public class AppTests
{
    private readonly App app_0 = new App();

    [Fact]
    public void Should_not_annotate_with_null_label()
    {
        var app_1 = app_0.Annotate(label: null);

        Assert.Same(app_1, app_0);
    }

    [Fact]
    public void Should_annotate_with_label()
    {
        var newLabel = "My App";

        var app_1 = app_0.Annotate(label: newLabel);
        var app_2 = app_1.Annotate(label: newLabel);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newLabel, app_1.Label);
        Assert.Equal(newLabel, app_2.Label);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_not_annotate_with_null_description()
    {
        var app_1 = app_0.Annotate(description: null);

        Assert.Same(app_1, app_0);
    }

    [Fact]
    public void Should_annotate_with_description()
    {
        var newDescription = "My Description";

        var app_1 = app_0.Annotate(description: newDescription);
        var app_2 = app_1.Annotate(description: newDescription);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newDescription, app_1.Description);
        Assert.Equal(newDescription, app_2.Description);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_transer()
    {
        var newTeamId = DomainId.NewGuid();

        var app_1 = app_0.Transfer(newTeamId);
        var app_2 = app_1.Transfer(newTeamId);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newTeamId, app_1.TeamId);
        Assert.Equal(newTeamId, app_2.TeamId);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_plan()
    {
        var newPlan1 = new AssignedPlan(RefToken.User("me"), "Premium");
        var newPlan2 = new AssignedPlan(RefToken.User("me"), "Premium");

        var app_1 = app_0.ChangePlan(newPlan1);
        var app_2 = app_1.ChangePlan(newPlan2);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newPlan1, app_1.Plan);
        Assert.Equal(newPlan2, app_2.Plan);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_image()
    {
        var newImage1 = new AppImage("image/png", "42");
        var newImage2 = new AppImage("image/png", "42");

        var app_1 = app_0.UpdateImage(newImage1);
        var app_2 = app_1.UpdateImage(newImage2);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newImage1, app_1.Image);
        Assert.Equal(newImage1, app_2.Image);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_settings()
    {
        var newSettings1 = new AppSettings { HideDateTimeModeButton = true };
        var newSettings2 = new AppSettings { HideDateTimeModeButton = true };

        var app_1 = app_0.UpdateSettings(newSettings1);
        var app_2 = app_1.UpdateSettings(newSettings2);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newSettings1, app_1.Settings);
        Assert.Equal(newSettings1, app_2.Settings);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_schema_scripts()
    {
        var newScripts1 = new AssetScripts { Query = "query" };
        var newScripts2 = new AssetScripts { Query = "query" };

        var app_1 = app_0.UpdateAssetScripts(newScripts1);
        var app_2 = app_1.UpdateAssetScripts(newScripts2);

        Assert.NotSame(app_0, app_1);
        Assert.Equal(newScripts1, app_1.AssetScripts);
        Assert.Equal(newScripts1, app_2.AssetScripts);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_contributors()
    {
        var app_1 = app_0.UpdateContributors(true, (_, c) => c.Assign("me", Role.Owner));
        var app_2 = app_1.UpdateContributors(true, (_, c) => c.Assign("me", Role.Owner));

        Assert.NotSame(app_0, app_1);
        Assert.Single(app_1.Contributors);
        Assert.Single(app_2.Contributors);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_clients()
    {
        var newClient = "MyClient";

        var app_1 = app_0.UpdateClients(true, (_, c) => c.Add(newClient, newClient));
        var app_2 = app_1.UpdateClients(true, (_, c) => c.Add(newClient, newClient));

        Assert.NotSame(app_0, app_1);
        Assert.True(app_1.Clients.ContainsKey(newClient));
        Assert.True(app_2.Clients.ContainsKey(newClient));
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_languages()
    {
        var newLanguage = Language.DE;

        var app_1 = app_0.UpdateLanguages(true, (_, l) => l.Set(newLanguage));
        var app_2 = app_1.UpdateLanguages(true, (_, l) => l.Set(newLanguage));

        Assert.NotSame(app_0, app_1);
        Assert.True(app_1.Languages.Contains(newLanguage));
        Assert.True(app_2.Languages.Contains(newLanguage));
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_roles()
    {
        var newRole = "MyRole";

        var app_1 = app_0.UpdateRoles(true, (_, r) => r.Add(newRole));
        var app_2 = app_1.UpdateRoles(true, (_, r) => r.Add(newRole));

        Assert.NotSame(app_0, app_1);
        Assert.Contains(app_1.Roles.Custom, x => x.Name == newRole);
        Assert.Contains(app_2.Roles.Custom, x => x.Name == newRole);
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_update_workflows()
    {
        var newWorkflowName = "MyWorkflow";
        var newWorkflowId = DomainId.NewGuid();

        var app_1 = app_0.UpdateWorkflows(true, (_, r) => r.Add(newWorkflowId, newWorkflowName));
        var app_2 = app_1.UpdateWorkflows(true, (_, r) => r.Add(newWorkflowId, newWorkflowName));

        Assert.NotSame(app_0, app_1);
        Assert.True(app_1.Workflows.ContainsKey(newWorkflowId));
        Assert.True(app_2.Workflows.ContainsKey(newWorkflowId));
        Assert.Same(app_1, app_2);
    }

    [Fact]
    public void Should_deserialize_old_state()
    {
        var original = TestUtils.DefaultSerializer.Deserialize<App>(File.ReadAllText("Model/Apps/App.json"));

        var deserialized = TestUtils.DefaultSerializer.Deserialize<App>(File.ReadAllText("Model/Apps/App_Old.json"));

        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Apps/App.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<App>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Apps/App.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<App>(json));

        Assert.Equal(json, serialized);
    }
}
