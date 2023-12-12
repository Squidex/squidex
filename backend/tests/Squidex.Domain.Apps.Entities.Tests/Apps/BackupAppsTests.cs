// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps;

public class BackupAppsTests : GivenContext
{
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly IAppsIndex appsIndex = A.Fake<IAppsIndex>();
    private readonly IAppUISettings appUISettings = A.Fake<IAppUISettings>();
    private readonly IAppImageStore appImageStore = A.Fake<IAppImageStore>();
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly BackupApps sut;

    public BackupAppsTests()
    {
        sut = new BackupApps(rebuilder, appImageStore, appProvider, appsIndex, appUISettings);
    }

    [Fact]
    public void Should_provide_name()
    {
        Assert.Equal("Apps", sut.Name);
    }

    [Fact]
    public async Task Should_reserve_app_name()
    {
        var context = CreateRestoreContext();

        A.CallTo(() => appsIndex.ReserveAsync(AppId.Id, AppId.Name, A<CancellationToken>._))
            .Returns("Reservation");

        await sut.RestoreEventAsync(Envelope.Create(new AppCreated
        {
            Name = AppId.Name
        }), context, CancellationToken);

        A.CallTo(() => appsIndex.ReserveAsync(AppId.Id, AppId.Name, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_complete_reservation_with_previous_token()
    {
        var appObject = A.Fake<AppDomainObject>();

        var context = CreateRestoreContext();

        A.CallTo(() => appObject.Snapshot)
            .Returns(App);

        A.CallTo(() => rebuilder.RebuildStateAsync<AppDomainObject, App>(context.AppId, CancellationToken))
            .Returns(appObject);

        A.CallTo(() => appsIndex.ReserveAsync(AppId.Id, AppId.Name, CancellationToken))
            .Returns("Reservation");

        await sut.RestoreEventAsync(Envelope.Create(new AppCreated
        {
            Name = AppId.Name
        }), context, CancellationToken);

        await sut.RestoreAsync(context, CancellationToken);
        await sut.CompleteRestoreAsync(context, AppId.Name);

        A.CallTo(() => appsIndex.RemoveReservationAsync("Reservation", default))
            .MustHaveHappened();

        A.CallTo(() => appObject.RebuildStateAsync(default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_cleanup_reservation_with_previous_token()
    {
        var context = CreateRestoreContext();

        A.CallTo(() => appsIndex.ReserveAsync(AppId.Id, AppId.Name, CancellationToken))
            .Returns("Reservation");

        await sut.RestoreEventAsync(Envelope.Create(new AppCreated
        {
            Name = AppId.Name
        }), context, CancellationToken);

        await sut.CleanupRestoreErrorAsync(AppId.Id);

        A.CallTo(() => appsIndex.RemoveReservationAsync("Reservation", default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_no_reservation_token_returned()
    {
        var context = CreateRestoreContext();

        A.CallTo(() => appsIndex.ReserveAsync(AppId.Id, AppId.Name, CancellationToken))
            .Returns(Task.FromResult<string?>(null));

        var @event = Envelope.Create(new AppCreated
        {
            Name = AppId.Name
        });

        await Assert.ThrowsAsync<BackupRestoreException>(() => sut.RestoreEventAsync(@event, context, CancellationToken));
    }

    [Fact]
    public async Task Should_not_cleanup_reservation_if_no_reservation_token_hold()
    {
        await sut.CleanupRestoreErrorAsync(AppId.Id);

        A.CallTo(() => appsIndex.RemoveReservationAsync("Reservation", A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_write_user_settings()
    {
        var settings = JsonValue.Object();

        var context = CreateBackupContext();

        A.CallTo(() => appUISettings.GetAsync(AppId.Id, null, CancellationToken))
            .Returns(settings);

        await sut.BackupAsync(context, CancellationToken);

        A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, settings, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_register_app_to_provider()
    {
        var appObject = A.Fake<AppDomainObject>();

        var context = CreateRestoreContext();

        A.CallTo(() => appObject.Snapshot)
            .Returns(App);

        A.CallTo(() => rebuilder.RebuildStateAsync<AppDomainObject, App>(context.AppId, CancellationToken))
            .Returns(appObject);

        await sut.RestoreAsync(context, CancellationToken);

        A.CallTo(() => appProvider.RegisterAppForLocalContext(context.AppId, appObject.Snapshot))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_read_user_settings()
    {
        var settings = JsonValue.Object();

        var context = CreateRestoreContext();

        A.CallTo(() => context.Reader.ReadJsonAsync<JsonObject>(A<string>._, CancellationToken))
            .Returns(settings);

        await sut.RestoreAsync(context, CancellationToken);

        A.CallTo(() => appUISettings.SetAsync(AppId.Id, null, settings, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_map_contributor_id_if_assigned()
    {
        var context = CreateRestoreContext();

        var @event = Envelope.Create(new AppContributorAssigned
        {
            ContributorId = "found"
        });

        var actual = await sut.RestoreEventAsync(@event, context, CancellationToken);

        Assert.True(actual);
        Assert.Equal("found_mapped", @event.Payload.ContributorId);
    }

    [Fact]
    public async Task Should_ignore_contributor_event_if_assigned_user_not_mapped()
    {
        var context = CreateRestoreContext();

        var @event = Envelope.Create(new AppContributorAssigned
        {
            ContributorId = "unknown"
        });

        var actual = await sut.RestoreEventAsync(@event, context, CancellationToken);

        Assert.False(actual);
        Assert.Equal("unknown", @event.Payload.ContributorId);
    }

    [Fact]
    public async Task Should_map_contributor_id_if_revoked()
    {
        var context = CreateRestoreContext();

        var @event = Envelope.Create(new AppContributorRemoved
        {
            ContributorId = "found"
        });

        var actual = await sut.RestoreEventAsync(@event, context, CancellationToken);

        Assert.True(actual);
        Assert.Equal("found_mapped", @event.Payload.ContributorId);
    }

    [Fact]
    public async Task Should_ignore_contributor_event_if_removed_user_not_mapped()
    {
        var context = CreateRestoreContext();

        var @event = Envelope.Create(new AppContributorRemoved
        {
            ContributorId = "unknown"
        });

        var actual = await sut.RestoreEventAsync(@event, context, CancellationToken);

        Assert.False(actual);
        Assert.Equal("unknown", @event.Payload.ContributorId);
    }

    [Fact]
    public async Task Should_ignore_exception_if_app_image_to_backup_does_not_exist()
    {
        var imageStream = new MemoryStream();

        var context = CreateBackupContext();

        A.CallTo(() => context.Writer.OpenBlobAsync(A<string>._, CancellationToken))
            .Returns(imageStream);

        A.CallTo(() => appImageStore.DownloadAsync(AppId.Id, imageStream, CancellationToken))
            .Throws(new AssetNotFoundException("Image"));

        await sut.BackupEventAsync(Envelope.Create(new AppImageUploaded()), context, CancellationToken);
    }

    [Fact]
    public async Task Should_backup_app_image()
    {
        var imageStream = new MemoryStream();

        var context = CreateBackupContext();

        A.CallTo(() => context.Writer.OpenBlobAsync(A<string>._, CancellationToken))
            .Returns(imageStream);

        await sut.BackupEventAsync(Envelope.Create(new AppImageUploaded()), context, CancellationToken);

        A.CallTo(() => appImageStore.DownloadAsync(AppId.Id, imageStream, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_restore_app_image()
    {
        var imageStream = new MemoryStream();

        var context = CreateRestoreContext();

        A.CallTo(() => context.Reader.OpenBlobAsync(A<string>._, CancellationToken))
            .Returns(imageStream);

        await sut.RestoreEventAsync(Envelope.Create(new AppImageUploaded()), context, CancellationToken);

        A.CallTo(() => appImageStore.UploadAsync(AppId.Id, imageStream, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_exception_if_app_image_cannot_be_overriden()
    {
        var imageStream = new MemoryStream();

        var context = CreateRestoreContext();

        A.CallTo(() => context.Reader.OpenBlobAsync(A<string>._, CancellationToken))
            .Returns(imageStream);

        A.CallTo(() => appImageStore.UploadAsync(AppId.Id, imageStream, CancellationToken))
            .Throws(new AssetAlreadyExistsException("Image"));

        await sut.RestoreEventAsync(Envelope.Create(new AppImageUploaded()), context, CancellationToken);
    }

    private BackupContext CreateBackupContext()
    {
        return new BackupContext(AppId.Id, CreateUserMapping(), A.Fake<IBackupWriter>());
    }

    private RestoreContext CreateRestoreContext()
    {
        return new RestoreContext(AppId.Id, CreateUserMapping(), A.Fake<IBackupReader>(), DomainId.NewGuid());
    }

    private IUserMapping CreateUserMapping()
    {
        var mapping = A.Fake<IUserMapping>();

        A.CallTo(() => mapping.Initiator)
            .Returns(User);

        RefToken mapped;

        A.CallTo(() => mapping.TryMap(A<string>.That.Matches(x => x.StartsWith("found", StringComparison.OrdinalIgnoreCase)), out mapped))
            .Returns(true)
            .AssignsOutAndRefParametersLazily(
                new Func<string, RefToken, object[]>((x, _) =>
                    new[] { RefToken.User($"{x}_mapped") }));

        A.CallTo(() => mapping.TryMap("notfound", out mapped))
            .Returns(false);

        return mapping;
    }
}
