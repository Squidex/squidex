// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public class BackupRulesTests : GivenContext
{
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly BackupRules sut;

    public BackupRulesTests()
    {
        sut = new BackupRules(rebuilder);
    }

    [Fact]
    public void Should_provide_name()
    {
        Assert.Equal("Rules", sut.Name);
    }

    [Fact]
    public async Task Should_restore_indices_for_all_non_deleted_rules()
    {
        var ruleId1 = DomainId.NewGuid();
        var ruleId2 = DomainId.NewGuid();
        var ruleId3 = DomainId.NewGuid();

        var context = new RestoreContext(AppId.Id, new UserMapping(RefToken.User("123")), A.Fake<IBackupReader>(), DomainId.NewGuid());

        await sut.RestoreEventAsync(AppEvent(new RuleCreated
        {
            RuleId = ruleId1
        }), context, CancellationToken);

        await sut.RestoreEventAsync(AppEvent(new RuleCreated
        {
            RuleId = ruleId2
        }), context, CancellationToken);

        await sut.RestoreEventAsync(AppEvent(new RuleCreated
        {
            RuleId = ruleId3
        }), context, CancellationToken);

        await sut.RestoreEventAsync(AppEvent(new RuleDeleted
        {
            RuleId = ruleId3
        }), context, CancellationToken);

        var rebuildAssets = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<RuleDomainObject, Rule>(A<IEnumerable<DomainId>>._, A<int>._, CancellationToken))
            .Invokes(x => rebuildAssets.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, CancellationToken);

        Assert.Equal(
        [
            DomainId.Combine(AppId, ruleId1),
            DomainId.Combine(AppId, ruleId2)
        ], rebuildAssets);
    }

    private Envelope<RuleEvent> AppEvent(RuleEvent @event)
    {
        @event.AppId = AppId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(AppId, @event.RuleId));
    }
}
