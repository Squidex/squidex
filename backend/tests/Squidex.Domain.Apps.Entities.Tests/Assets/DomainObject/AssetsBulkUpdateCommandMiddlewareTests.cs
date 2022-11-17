// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public class AssetsBulkUpdateCommandMiddlewareTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
    private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly AssetsBulkUpdateCommandMiddleware sut;

    public AssetsBulkUpdateCommandMiddlewareTests()
    {
        ct = cts.Token;

        var log = A.Fake<ILogger<AssetsBulkUpdateCommandMiddleware>>();

        sut = new AssetsBulkUpdateCommandMiddleware(contextProvider, log);
    }

    [Fact]
    public async Task Should_do_nothing_if_jobs_is_null()
    {
        var command = new BulkUpdateAssets();

        var actual = await PublishAsync(command);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_do_nothing_if_jobs_is_empty()
    {
        var command = new BulkUpdateAssets { Jobs = Array.Empty<BulkUpdateJob>() };

        var actual = await PublishAsync(command);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_annotate_asset()
    {
        SetupContext(PermissionIds.AppAssetsUpdate);

        var id = DomainId.NewGuid();

        var command = BulkCommand(BulkUpdateAssetType.Annotate, id, fileName: "file");

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(A<AnnotateAsset>.That.Matches(x => x.AssetId == id && x.FileName == "file"), ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_annotating()
    {
        SetupContext(PermissionIds.AppAssetsRead);

        var id = DomainId.NewGuid();

        var command = BulkCommand(BulkUpdateAssetType.Move, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_move_asset()
    {
        SetupContext(PermissionIds.AppAssetsUpdate);

        var id = DomainId.NewGuid();

        var command = BulkCommand(BulkUpdateAssetType.Move, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(A<MoveAsset>.That.Matches(x => x.AssetId == id), ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_moving()
    {
        SetupContext(PermissionIds.AppAssetsRead);

        var id = DomainId.NewGuid();

        var command = BulkCommand(BulkUpdateAssetType.Move, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_delete_asset()
    {
        SetupContext(PermissionIds.AppAssetsDelete);

        var id = DomainId.NewGuid();

        var command = BulkCommand(BulkUpdateAssetType.Delete, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<DeleteAsset>.That.Matches(x => x.AssetId == id), ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_deletion()
    {
        SetupContext(PermissionIds.AppAssetsRead);

        var id = DomainId.NewGuid();

        var command = BulkCommand(BulkUpdateAssetType.Delete, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private async Task<BulkUpdateResult> PublishAsync(ICommand command)
    {
        var context = new CommandContext(command, commandBus);

        await sut.HandleAsync(context, ct);

        return (context.PlainResult as BulkUpdateResult)!;
    }

    private BulkUpdateAssets BulkCommand(BulkUpdateAssetType type, DomainId id, string? fileName = null)
    {
        return new BulkUpdateAssets
        {
            AppId = appId,
            Jobs = new[]
            {
                new BulkUpdateJob
                {
                    Type = type,
                    Id = id,
                    FileName = fileName
                }
            }
        };
    }

    private Context SetupContext(string id)
    {
        var permission = PermissionIds.ForApp(id, appId.Name).Id;

        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));

        var requestContext = new Context(claimsPrincipal, Mocks.App(appId));

        A.CallTo(() => contextProvider.Context)
            .Returns(requestContext);

        return requestContext;
    }
}
