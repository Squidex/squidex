// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public sealed class ScriptingExtensionsTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly RefToken actor = RefToken.User("123");

    [Fact]
    public async Task Should_add_tag_in_script()
    {
        var script = "ctx.command.tags.add('tag');";

        var command = new AnnotateAsset { Tags = new HashSet<string>() };

        var operation = Operation(script, CreateAsset(), command);

        await operation.ExecuteAnnotateScriptAsync(command);

        Assert.Contains("tag", command.Tags);
    }

    [Fact]
    public async Task Should_add_metadata_in_script()
    {
        var script = "ctx.command.metadata['foo'] = 42;";

        var command = new AnnotateAsset { Metadata = new AssetMetadata() };

        var operation = Operation(script, CreateAsset(), command);

        await operation.ExecuteAnnotateScriptAsync(command);

        Assert.Equal(JsonValue.Create(42), command.Metadata["foo"]);
    }

    [Fact]
    public async Task Should_not_allow_to_write_asset_tags()
    {
        var script = "ctx.asset.tags.add('tag');";

        var command = new AnnotateAsset();

        var operation = Operation(script, CreateAsset(), command);

        await Assert.ThrowsAsync<ValidationException>(() => operation.ExecuteAnnotateScriptAsync(command));
    }

    [Fact]
    public async Task Should_not_allow_to_write_asset_metadata()
    {
        var script = "ctx.asset.metadata['foo'] = 42;";

        var command = new AnnotateAsset();

        var operation = Operation(script, CreateAsset(), command);

        await Assert.ThrowsAsync<ValidationException>(() => operation.ExecuteAnnotateScriptAsync(command));
    }

    private AssetOperation Operation(string script, AssetEntity asset, AnnotateAsset command)
    {
        var scripts = new AssetScripts
        {
            Annotate = script
        };

        var app = Mocks.App(appId);

        A.CallTo(() => app.AssetScripts)
            .Returns(scripts);

        var serviceProvider =
            new ServiceCollection()
                .AddMemoryCache()
                .AddOptions()
                .AddOptions<MemoryCacheOptions>().Services
                .AddSingleton<IScriptEngine, JintScriptEngine>()
                .BuildServiceProvider();

        command.Actor = actor;
        command.User = Mocks.FrontendUser();

        return new AssetOperation(serviceProvider, () => asset)
        {
            App = app,
            CommandId = asset.Id,
            Command = command
        };
    }

    private AssetEntity CreateAsset()
    {
        return new AssetEntity
        {
            Id = DomainId.NewGuid(),
            AppId = appId,
            Created = default,
            CreatedBy = actor,
            Metadata = new AssetMetadata(),
            Tags = new HashSet<string>()
        };
    }
}
