// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class BackupAppsTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
        private readonly IAppsIndex appsIndex = A.Fake<IAppsIndex>();
        private readonly IAppUISettings appUISettings = A.Fake<IAppUISettings>();
        private readonly IAppImageStore appImageStore = A.Fake<IAppImageStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly RefToken actor = RefToken.User("123");
        private readonly BackupApps sut;

        public BackupAppsTests()
        {
            ct = cts.Token;

            sut = new BackupApps(rebuilder, appImageStore, appsIndex,  appUISettings);
        }

        [Fact]
        public void Should_provide_name()
        {
            Assert.Equal("Apps", sut.Name);
        }

        [Fact]
        public async Task Should_reserve_app_name()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => appsIndex.ReserveAsync(appId, appName, A<CancellationToken>._))
                .Returns("Reservation");

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context, ct);

            A.CallTo(() => appsIndex.ReserveAsync(appId, appName, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_complete_reservation_with_previous_token()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => appsIndex.ReserveAsync(appId, appName, ct))
                .Returns("Reservation");

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context, ct);

            await sut.CompleteRestoreAsync(context);

            A.CallTo(() => appsIndex.RemoveReservationAsync("Reservation", default))
                .MustHaveHappened();

            A.CallTo(() => rebuilder.InsertManyAsync<AppDomainObject, AppDomainObject.State>(A<IEnumerable<DomainId>>.That.Is(appId), 1, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cleanup_reservation_with_previous_token()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => appsIndex.ReserveAsync(appId, appName, ct))
                .Returns("Reservation");

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context, ct);

            await sut.CleanupRestoreErrorAsync(appId);

            A.CallTo(() => appsIndex.RemoveReservationAsync("Reservation", default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_no_reservation_token_returned()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => appsIndex.ReserveAsync(appId, appName, ct))
                .Returns(Task.FromResult<string?>(null));

            var @event = Envelope.Create(new AppCreated
            {
                Name = appName
            });

            await Assert.ThrowsAsync<BackupRestoreException>(() => sut.RestoreEventAsync(@event, context, ct));
        }

        [Fact]
        public async Task Should_not_cleanup_reservation_if_no_reservation_token_hold()
        {
            await sut.CleanupRestoreErrorAsync(appId);

            A.CallTo(() => appsIndex.RemoveReservationAsync("Reservation", ct))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_writer_user_settings()
        {
            var settings = JsonValue.Object();

            var context = CreateBackupContext();

            A.CallTo(() => appUISettings.GetAsync(appId, null))
                .Returns(settings);

            await sut.BackupAsync(context, ct);

            A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, settings, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_read_user_settings()
        {
            var settings = JsonValue.Object();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadJsonAsync<JsonObject>(A<string>._, ct))
                .Returns(settings);

            await sut.RestoreAsync(context, ct);

            A.CallTo(() => appUISettings.SetAsync(appId, null, settings))
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

            var result = await sut.RestoreEventAsync(@event, context, ct);

            Assert.True(result);
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

            var result = await sut.RestoreEventAsync(@event, context, ct);

            Assert.False(result);
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

            var result = await sut.RestoreEventAsync(@event, context, ct);

            Assert.True(result);
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

            var result = await sut.RestoreEventAsync(@event, context, ct);

            Assert.False(result);
            Assert.Equal("unknown", @event.Payload.ContributorId);
        }

        [Fact]
        public async Task Should_ignore_exception_if_app_image_to_backup_does_not_exist()
        {
            var imageStream = new MemoryStream();

            var context = CreateBackupContext();

            A.CallTo(() => context.Writer.OpenBlobAsync(A<string>._, ct))
                .Returns(imageStream);

            A.CallTo(() => appImageStore.DownloadAsync(appId, imageStream, ct))
                .Throws(new AssetNotFoundException("Image"));

            await sut.BackupEventAsync(Envelope.Create(new AppImageUploaded()), context, ct);
        }

        [Fact]
        public async Task Should_backup_app_image()
        {
            var imageStream = new MemoryStream();

            var context = CreateBackupContext();

            A.CallTo(() => context.Writer.OpenBlobAsync(A<string>._, ct))
                .Returns(imageStream);

            await sut.BackupEventAsync(Envelope.Create(new AppImageUploaded()), context, ct);

            A.CallTo(() => appImageStore.DownloadAsync(appId, imageStream, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_restore_app_image()
        {
            var imageStream = new MemoryStream();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.OpenBlobAsync(A<string>._, ct))
                .Returns(imageStream);

            await sut.RestoreEventAsync(Envelope.Create(new AppImageUploaded()), context, ct);

            A.CallTo(() => appImageStore.UploadAsync(appId, imageStream, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_exception_if_app_image_cannot_be_overriden()
        {
            var imageStream = new MemoryStream();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.OpenBlobAsync(A<string>._, ct))
                .Returns(imageStream);

            A.CallTo(() => appImageStore.UploadAsync(appId, imageStream, ct))
                .Throws(new AssetAlreadyExistsException("Image"));

            await sut.RestoreEventAsync(Envelope.Create(new AppImageUploaded()), context, ct);
        }

        private BackupContext CreateBackupContext()
        {
            return new BackupContext(appId, CreateUserMapping(), A.Fake<IBackupWriter>());
        }

        private RestoreContext CreateRestoreContext()
        {
            return new RestoreContext(appId, CreateUserMapping(), A.Fake<IBackupReader>(), DomainId.NewGuid());
        }

        private IUserMapping CreateUserMapping()
        {
            var mapping = A.Fake<IUserMapping>();

            A.CallTo(() => mapping.Initiator).Returns(actor);

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
}
