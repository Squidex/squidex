// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Infrastructure.Orleans;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public class UsageNotifierGrainTests
    {
        private readonly IGrainState<UsageNotifierGrain.State> state = A.Fake<IGrainState<UsageNotifierGrain.State>>();
        private readonly INotificationSender notificationSender = A.Fake<INotificationSender>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IClock clock = A.Fake<IClock>();
        private readonly UsageNotifierGrain sut;
        private Instant time = SystemClock.Instance.GetCurrentInstant();

        public UsageNotifierGrainTests()
        {
            A.CallTo(() => clock.GetCurrentInstant())
                .ReturnsLazily(() => time);

            A.CallTo(() => notificationSender.IsActive)
                .Returns(true);

            sut = new UsageNotifierGrain(state, notificationSender, userResolver, clock);
        }

        [Fact]
        public async Task Should_not_send_notification_if_not_active()
        {
            SetupUser("1", null);
            SetupUser("2", null);

            var notification = new UsageNotification
            {
                AppName = "my-app",
                Usage = 1000,
                UsageLimit = 3000,
                Users = new[] { "1", "2" }
            };

            await sut.NotifyAsync(notification);

            A.CallTo(() => notificationSender.SendUsageAsync(A<IUser>._, "my-app", 1000, 3000))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_notify_found_users()
        {
            var user1 = SetupUser("1", "user1@email.com");
            var user2 = SetupUser("2", "user2@email.com");
            var user3 = SetupUser("3", null);

            var notification = new UsageNotification
            {
                AppName = "my-app",
                Usage = 1000,
                UsageLimit = 3000,
                Users = new[] { "1", "2", "3" }
            };

            await sut.NotifyAsync(notification);

            A.CallTo(() => notificationSender.SendUsageAsync(user1!, "my-app", 1000, 3000))
                .MustHaveHappened();

            A.CallTo(() => notificationSender.SendUsageAsync(user2!, "my-app", 1000, 3000))
                .MustHaveHappened();

            A.CallTo(() => notificationSender.SendUsageAsync(user3!, "my-app", 1000, 3000))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_notify_again()
        {
            var user = SetupUser("1", "user1@email.com");

            var notification = new UsageNotification
            {
                AppName = "my-app",
                Usage = 1000,
                UsageLimit = 3000,
                Users = new[] { "1" }
            };

            await sut.NotifyAsync(notification);
            await sut.NotifyAsync(notification);

            A.CallTo(() => notificationSender.SendUsageAsync(user!, "my-app", 1000, 3000))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_send_again_after_3_days()
        {
            var user = SetupUser("1", "user1@email.com");

            var notification = new UsageNotification
            {
                AppName = "my-app",
                Usage = 1000,
                UsageLimit = 3000,
                Users = new[] { "1" }
            };

            await sut.NotifyAsync(notification);

            time = time.Plus(Duration.FromDays(3));

            await sut.NotifyAsync(notification);

            A.CallTo(() => notificationSender.SendUsageAsync(user!, "my-app", 1000, 3000))
                .MustHaveHappenedTwiceExactly();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        [InlineData(24)]
        [InlineData(48)]
        public async Task Should_not_notify_again_after_few_hours(int hours)
        {
            var user = SetupUser("1", "user1@email.com");

            var notification = new UsageNotification
            {
                AppName = "my-app",
                Usage = 1000,
                UsageLimit = 3000,
                Users = new[] { "1" }
            };

            await sut.NotifyAsync(notification);

            time = time.Plus(Duration.FromHours(hours));

            await sut.NotifyAsync(notification);

            A.CallTo(() => notificationSender.SendUsageAsync(user!, "my-app", 1000, 3000))
                .MustHaveHappenedOnceExactly();
        }

        private IUser? SetupUser(string id, string? email)
        {
            if (email != null)
            {
                var user = UserMocks.User(id, email);

                A.CallTo(() => userResolver.FindByIdOrEmailAsync(id, default))
                    .Returns(user);

                return user;
            }
            else
            {
                A.CallTo(() => userResolver.FindByIdOrEmailAsync(id, default))
                    .Returns(Task.FromResult<IUser?>(null));

                return null;
            }
        }
    }
}
