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
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public sealed class ScriptingExtensionsTests : GivenContext
{
    [Fact]
    public async Task Should_add_tag_in_script()
    {
        var script = "ctx.command.tags.add('tag');";

        var command = new AnnotateAsset { Tags = [] };

        var operation = Operation(script, CreateAsset(), command);

        await operation.ExecuteAnnotateScriptAsync(command, CancellationToken);

        Assert.Contains("tag", command.Tags);
    }

    [Fact]
    public async Task Should_add_metadata_in_script()
    {
        var script = "ctx.command.metadata['foo'] = 42;";

        var command = new AnnotateAsset { Metadata = [] };

        var operation = Operation(script, CreateAsset(), command);

        await operation.ExecuteAnnotateScriptAsync(command, CancellationToken);

        Assert.Equal(JsonValue.Create(42), command.Metadata["foo"]);
    }

    [Fact]
    public async Task Should_not_allow_to_write_asset_tags()
    {
        var script = "ctx.asset.tags.add('tag');";

        var command = new AnnotateAsset();

        var operation = Operation(script, CreateAsset(), command);

        await Assert.ThrowsAsync<ValidationException>(() => operation.ExecuteAnnotateScriptAsync(command, CancellationToken));
    }

    [Fact]
    public async Task Should_not_allow_to_write_asset_metadata()
    {
        var script = "ctx.asset.metadata['foo'] = 42;";

        var command = new AnnotateAsset();

        var operation = Operation(script, CreateAsset(), command);

        await Assert.ThrowsAsync<ValidationException>(() => operation.ExecuteAnnotateScriptAsync(command, CancellationToken));
    }

    private AssetOperation Operation(string script, Asset asset, AnnotateAsset command)
    {
        App = App with
        {
            AssetScripts = new AssetScripts
            {
                Annotate = script
            }
        };

        var serviceProvider =
            new ServiceCollection()
                .AddMemoryCache()
                .AddOptions()
                .AddOptions<MemoryCacheOptions>().Services
                .AddSingleton<IScriptEngine, JintScriptEngine>()
                .BuildServiceProvider();

        command.Actor = User;
        command.User = Mocks.FrontendUser();

        return new AssetOperation(serviceProvider, () => asset)
        {
            App = App,
            CommandId = asset.Id,
            Command = command
        };
    }
}
