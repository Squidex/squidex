// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class RuleEventDto : Resource
{
    /// <summary>
    /// The ID of the event.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The flow state.
    /// </summary>
    public FlowExecutionStateDto FlowState { get; set; }

    public static RuleEventDto FromDomain(FlowExecutionState<FlowEventContext> state, Resources resources)
    {
        var result = new RuleEventDto
        {
            Id = DomainId.Create(state.InstanceId),
            FlowState = FlowExecutionStateDto.FromDomain(state),
        };

        return result.CreateLinks(resources);
    }

    private RuleEventDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, id = Id };

        if (resources.CanUpdateRuleEvents)
        {
            AddPutLink("update",
                resources.Url<RulesController>(x => nameof(x.PutEvent), values));
        }

        if (resources.CanDeleteRuleEvents && FlowState.NextRun != null)
        {
            AddDeleteLink("cancel",
                resources.Url<RulesController>(x => nameof(x.DeleteEvent), values));
        }

        return this;
    }
}
