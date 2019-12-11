﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class BackupRulesTests
    {
        private readonly IRulesIndex index = A.Fake<IRulesIndex>();
        private readonly BackupRules sut;

        public BackupRulesTests()
        {
            sut = new BackupRules(index);
        }

        [Fact]
        public void Should_provide_name()
        {
            Assert.Equal("Rules", sut.Name);
        }

        [Fact]
        public async Task Should_restore_indices_for_all_non_deleted_rules()
        {
            var appId = Guid.NewGuid();

            var ruleId1 = Guid.NewGuid();
            var ruleId2 = Guid.NewGuid();
            var ruleId3 = Guid.NewGuid();

            var context = new RestoreContext(appId, new UserMapping(new RefToken(RefTokenType.Subject, "123")), A.Fake<IBackupReader>());

            await sut.RestoreEventAsync(Envelope.Create(new RuleCreated
            {
                RuleId = ruleId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new RuleCreated
            {
                RuleId = ruleId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new RuleCreated
            {
                RuleId = ruleId3
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new RuleDeleted
            {
                RuleId = ruleId3
            }), context);

            HashSet<Guid>? newIndex = null;

            A.CallTo(() => index.RebuildAsync(appId, A<HashSet<Guid>>.Ignored))
                .Invokes(new Action<Guid, HashSet<Guid>>((_, i) => newIndex = i));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<Guid>
            {
                ruleId1,
                ruleId2,
            }, newIndex);
        }
    }
}
