// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class BackupAppsTests
    {
        private readonly IAppsIndex index = A.Fake<IAppsIndex>();
        private readonly IAppUISettings appUISettings = A.Fake<IAppUISettings>();
        private readonly IAppImageStore appImageStore = A.Fake<IAppImageStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly RefToken actor = RefToken.User("123");
        private readonly BackupApps sut;

        public BackupAppsTests()
        {
            sut = new BackupApps(appImageStore, index,  appUISettings);
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

            A.CallTo(() => index.ReserveAsync(appId, appName))
                .Returns("Reservation");

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context);

            A.CallTo(() => index.ReserveAsync(appId, appName))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_complete_reservation_with_previous_token()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => index.ReserveAsync(appId, appName))
                .Returns("Reservation");

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context);

            await sut.CompleteRestoreAsync(context);

            A.CallTo(() => index.AddAsync("Reservation"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cleanup_reservation_with_previous_token()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => index.ReserveAsync(appId, appName))
                .Returns("Reservation");

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context);

            await sut.CleanupRestoreErrorAsync(appId);

            A.CallTo(() => index.RemoveReservationAsync("Reservation"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_no_reservation_token_returned()
        {
            const string appName = "my-app";

            var context = CreateRestoreContext();

            A.CallTo(() => index.ReserveAsync(appId, appName))
                .Returns(Task.FromResult<string?>(null));

            await Assert.ThrowsAsync<BackupRestoreException>(() =>
            {
                return sut.RestoreEventAsync(Envelope.Create(new AppCreated
                {
                    Name = appName
                }), context);
            });
        }

        [Fact]
        public async Task Should_not_cleanup_reservation_if_no_reservation_token_hold()
        {
            await sut.CleanupRestoreErrorAsync(appId);

            A.CallTo(() => index.RemoveReservationAsync("Reservation"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_writer_user_settings()
        {
            var settings = JsonValue.Object();

            var context = CreateBackupContext();

            A.CallTo(() => appUISettings.GetAsync(appId, null))
                .Returns(settings);

            await sut.BackupAsync(context);

            A.CallTo(() => context.Writer.WriteJsonAsync(A<string>._, settings))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_read_user_settings()
        {
            var settings = JsonValue.Object();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadJsonAsync<JsonObject>(A<string>._))
                .Returns(settings);

            await sut.RestoreAsync(context);

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

            var result = await sut.RestoreEventAsync(@event, context);

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

            var result = await sut.RestoreEventAsync(@event, context);

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

            var result = await sut.RestoreEventAsync(@event, context);

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

            var result = await sut.RestoreEventAsync(@event, context);

            Assert.False(result);
            Assert.Equal("unknown", @event.Payload.ContributorId);
        }

        [Fact]
        public async Task Should_ignore_exception_if_app_image_to_backup_does_not_exist()
        {
            var imageStream = new MemoryStream();

            var context = CreateBackupContext();

            A.CallTo(() => context.Writer.WriteBlobAsync(A<string>._, A<Func<Stream, Task>>._))
                .Invokes((string _, Func<Stream, Task> handler) => handler(imageStream));

            A.CallTo(() => appImageStore.DownloadAsync(appId, imageStream, default))
                .Throws(new AssetNotFoundException("Image"));

            await sut.BackupEventAsync(Envelope.Create(new AppImageUploaded()), context);
        }

        [Fact]
        public async Task Should_backup_app_image()
        {
            var imageStream = new MemoryStream();

            var context = CreateBackupContext();

            A.CallTo(() => context.Writer.WriteBlobAsync(A<string>._, A<Func<Stream, Task>>._))
                .Invokes((string _, Func<Stream, Task> handler) => handler(imageStream));

            await sut.BackupEventAsync(Envelope.Create(new AppImageUploaded()), context);

            A.CallTo(() => appImageStore.DownloadAsync(appId, imageStream, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_restore_app_image()
        {
            var imageStream = new MemoryStream();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadBlobAsync(A<string>._, A<Func<Stream, Task>>._))
                .Invokes((string _, Func<Stream, Task> handler) => handler(imageStream));

            await sut.RestoreEventAsync(Envelope.Create(new AppImageUploaded()), context);

            A.CallTo(() => appImageStore.UploadAsync(appId, imageStream, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_exception_if_app_image_cannot_be_overriden()
        {
            var imageStream = new MemoryStream();

            var context = CreateRestoreContext();

            A.CallTo(() => context.Reader.ReadBlobAsync(A<string>._, A<Func<Stream, Task>>._))
                .Invokes((string _, Func<Stream, Task> handler) => handler(imageStream));

            A.CallTo(() => appImageStore.UploadAsync(appId, imageStream, default))
                .Throws(new AssetAlreadyExistsException("Image"));

            await sut.RestoreEventAsync(Envelope.Create(new AppImageUploaded()), context);
        }

        [Fact]
        public async Task Should_restore_indices_for_all_non_deleted_schemas()
        {
            var userId1 = "found1";
            var userId2 = "found2";
            var userId3 = "found3";
            var context = CreateRestoreContext();

            await sut.RestoreEventAsync(Envelope.Create(new AppContributorAssigned
            {
                ContributorId = userId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AppContributorAssigned
            {
                ContributorId = userId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AppContributorAssigned
            {
                ContributorId = userId3
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new AppContributorRemoved
            {
                ContributorId = userId3
            }), context);

            HashSet<string>? newIndex = null;

            A.CallTo(() => index.RebuildByContributorsAsync(appId, A<HashSet<string>>._))
                .Invokes(new Action<DomainId, HashSet<string>>((_, i) => newIndex = i));

            await sut.CompleteRestoreAsync(context);

            Assert.Equal(new HashSet<string>
            {
                "found1_mapped",
                "found2_mapped"
            }, newIndex);
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
