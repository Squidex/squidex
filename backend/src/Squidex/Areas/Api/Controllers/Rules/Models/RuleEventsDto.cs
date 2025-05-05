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

public sealed class RuleEventsDto : Resource
{
    /// <summary>
    /// The total number of rule events.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The rule events.
    /// </summary>
    public RuleEventDto[] Items { get; set; }

    public static RuleEventsDto FromDomain(IResultList<FlowExecutionState<FlowEventContext>> flowStates, Resources resources, DomainId? ruleId)
    {
        var result = new RuleEventsDto
        {
            Total = flowStates.Total,
            Items = flowStates.Select(x => RuleEventDto.FromDomain(x, resources)).ToArray(),
        };

        return result.CreateLinks(resources, ruleId);
    }

    private RuleEventsDto CreateLinks(Resources resources, DomainId? ruleId)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<RulesController>(x => nameof(x.GetEvents), values));

        if (resources.CanDeleteRuleEvents)
        {
            if (ruleId != null)
            {
                var routeValues = new { values.app, id = ruleId };

                AddDeleteLink("cancel",
                    resources.Url<RulesController>(x => nameof(x.DeleteRuleEvents), routeValues));
            }
            else
            {
                AddDeleteLink("cancel",
                    resources.Url<RulesController>(x => nameof(x.DeleteEvents), values));
            }
        }

        return this;
    }
}
