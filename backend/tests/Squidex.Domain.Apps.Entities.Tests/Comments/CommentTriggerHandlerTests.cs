// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public class CommentTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IRuleTriggerHandler sut;

        public CommentTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
                .Returns(false);

            sut = new CommentTriggerHandler(scriptEngine, userResolver);
        }

        [Fact]
        public void Should_return_false_when_asking_for_snapshot_support()
        {
            Assert.False(sut.CanCreateSnapshotEvents);
        }

        [Fact]
        public async Task Should_create_enriched_events()
        {
            var user1 = CreateUser("1");
            var user2 = CreateUser("2");

            var users = new List<IUser> { user1, user2 };
            var userIds = users.Select(x => x.Id).ToArray();

            var envelope = Envelope.Create<AppEvent>(new CommentCreated { Mentions = userIds });

            A.CallTo(() => userResolver.QueryManyAsync(userIds))
                .Returns(users.ToDictionary(x => x.Id));

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Equal(2, result.Count);

            var enrichedEvent1 = result[0] as EnrichedCommentEvent;
            var enrichedEvent2 = result[1] as EnrichedCommentEvent;

            Assert.Equal(user1, enrichedEvent1!.MentionedUser);
            Assert.Equal(user2, enrichedEvent2!.MentionedUser);
            Assert.Equal("UserMentioned", enrichedEvent1.Name);
            Assert.Equal("UserMentioned", enrichedEvent2.Name);
        }

        [Fact]
        public async Task Should_not_create_enriched_events_when_users_cannot_be_resolved()
        {
            var user1 = CreateUser("1");
            var user2 = CreateUser("2");

            var users = new List<IUser> { user1, user2 };
            var userIds = users.Select(x => x.Id).ToArray();

            var envelope = Envelope.Create<AppEvent>(new CommentCreated { Mentions = userIds });

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_not_create_enriched_events_when_mentions_is_null()
        {
            var envelope = Envelope.Create<AppEvent>(new CommentCreated { Mentions = null });

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Empty(result);

            A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_enriched_events_when_mentions_is_empty()
        {
            var envelope = Envelope.Create<AppEvent>(new CommentCreated { Mentions = Array.Empty<string>() });

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Empty(result);

            A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_skip_udated_event()
        {
            var envelope = Envelope.Create<AppEvent>(new CommentUpdated());

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Empty(result);

            A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_skip_deleted_event()
        {
            var envelope = Envelope.Create<AppEvent>(new CommentDeleted());

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Empty(result);

            A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_trigger_precheck_when_event_type_not_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new ContentCreated(), trigger, DomainId.NewGuid());

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_event_type_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new CommentCreated(), trigger, DomainId.NewGuid());

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_event_type_not_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent(), trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_is_empty()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new EnrichedCommentEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_matchs()
        {
            TestForCondition("true", trigger =>
            {
                var result = sut.Trigger(new EnrichedCommentEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_condition_does_not_matchs()
        {
            TestForCondition("false", trigger =>
            {
                var result = sut.Trigger(new EnrichedCommentEvent(), trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_email_is_correct()
        {
            TestForRealCondition("event.mentionedUser.email == 'sebastian@squidex.io'", (handler, trigger) =>
            {
                var user = CreateUser("1");

                var result = handler.Trigger(new EnrichedCommentEvent { MentionedUser = user }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_email_is_correct()
        {
            TestForRealCondition("event.mentionedUser.email == 'other@squidex.io'", (handler, trigger) =>
            {
                var user = CreateUser("1");

                var result = handler.Trigger(new EnrichedCommentEvent { MentionedUser = user }, trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_text_is_urgent()
        {
            TestForRealCondition("event.text.indexOf('urgent') >= 0", (handler, trigger) =>
            {
                var text = "Hey man, this is really urgent.";

                var result = handler.Trigger(new EnrichedCommentEvent { Text = text }, trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_text_is_not_urgent()
        {
            TestForRealCondition("event.text.indexOf('urgent') >= 0", (handler, trigger) =>
            {
                var text = "Hey man, just an information for you.";

                var result = handler.Trigger(new EnrichedCommentEvent { Text = text }, trigger);

                Assert.False(result);
            });
        }

        private static IUser CreateUser(string id, string email = "sebastian@squidex.io")
        {
            var user = A.Fake<IUser>();

            A.CallTo(() => user.Id).Returns(id);
            A.CallTo(() => user.Email).Returns(email);

            return user;
        }

        private void TestForRealCondition(string condition, Action<IRuleTriggerHandler, CommentTrigger> action)
        {
            var trigger = new CommentTrigger
            {
                Condition = condition
            };

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var handler = new CommentTriggerHandler(new JintScriptEngine(memoryCache), userResolver);

            action(handler, trigger);
        }

        private void TestForCondition(string condition, Action<CommentTrigger> action)
        {
            var trigger = new CommentTrigger
            {
                Condition = condition
            };

            action(trigger);

            if (string.IsNullOrWhiteSpace(condition))
            {
                A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                    .MustNotHaveHappened();
            }
            else
            {
                A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                    .MustHaveHappened();
            }
        }
    }
}
