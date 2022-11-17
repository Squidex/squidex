// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public class BackupRulesTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly BackupRules sut;

    public BackupRulesTests()
    {
        ct = cts.Token;

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

        var context = new RestoreContext(appId.Id, new UserMapping(RefToken.User("123")), A.Fake<IBackupReader>(), DomainId.NewGuid());

        await sut.RestoreEventAsync(AppEvent(new RuleCreated
        {
            RuleId = ruleId1
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new RuleCreated
        {
            RuleId = ruleId2
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new RuleCreated
        {
            RuleId = ruleId3
        }), context, ct);

        await sut.RestoreEventAsync(AppEvent(new RuleDeleted
        {
            RuleId = ruleId3
        }), context, ct);

        var rebuildAssets = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<RuleDomainObject, RuleDomainObject.State>(A<IEnumerable<DomainId>>._, A<int>._, ct))
            .Invokes(x => rebuildAssets.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, ct);

        Assert.Equal(new HashSet<DomainId>
        {
            DomainId.Combine(appId, ruleId1),
            DomainId.Combine(appId, ruleId2)
        }, rebuildAssets);
    }

    private Envelope<RuleEvent> AppEvent(RuleEvent @event)
    {
        @event.AppId = appId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(appId, @event.RuleId));
    }
}
