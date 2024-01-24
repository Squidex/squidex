// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public class AssetFolderDomainObjectTests : HandlerTestBase<AssetFolder>
{
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly DomainId parentId = DomainId.NewGuid();
    private readonly DomainId assetFolderId = DomainId.NewGuid();
    private readonly AssetFolderDomainObject sut;

    protected override DomainId Id
    {
        get => DomainId.Combine(AppId, assetFolderId);
    }

    public AssetFolderDomainObjectTests()
    {
        A.CallTo(() => assetQuery.FindAssetFolderAsync(AppId.Id, parentId, A<CancellationToken>._))
            .Returns(new List<AssetFolder> { A.Fake<AssetFolder>() });

        var log = A.Fake<ILogger<AssetFolderDomainObject>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
                .AddSingleton(assetQuery)
                .AddSingleton(contentRepository)
                .AddSingleton(log)
                .BuildServiceProvider();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new AssetFolderDomainObject(Id, PersistenceFactory, log, serviceProvider);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    [Fact]
    public async Task Command_should_throw_exception_if_rule_is_deleted()
    {
        await ExecuteCreateAsync();
        await ExecuteDeleteAsync();

        await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecuteUpdateAsync);
    }

    [Fact]
    public async Task Create_should_create_events_and_set_intitial_state()
    {
        var command = new CreateAssetFolder { FolderName = "New Name" };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Update_should_create_events_and_update_state()
    {
        var command = new RenameAssetFolder { FolderName = "New Name" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Move_should_create_events_and_update_state()
    {
        var command = new MoveAssetFolder { ParentId = parentId };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Delete_should_create_events_with_total_file_size()
    {
        var command = new DeleteAssetFolder();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(sut, new CreateAssetFolder { FolderName = "My Folder" });
    }

    private Task ExecuteUpdateAsync()
    {
        return PublishAsync(sut, new RenameAssetFolder { FolderName = "My Folder" });
    }

    private Task ExecuteDeleteAsync()
    {
        return PublishAsync(sut, new DeleteAssetFolder());
    }

    protected override IAggregateCommand CreateCommand(IAggregateCommand command)
    {
        ((AssetFolderCommand)command).AssetFolderId = assetFolderId;

        return base.CreateCommand(command);
    }

    private async Task VerifySutAsync(object? actual, object? expected = null)
    {
        if (expected == null)
        {
            actual.Should().BeEquivalentTo(sut.Snapshot, o => o.IncludingProperties());
        }
        else
        {
            actual.Should().BeEquivalentTo(expected);
        }

        Assert.Equal(AppId, sut.Snapshot.AppId);

        await Verify(new { sut, events = LastEvents });
    }
}
