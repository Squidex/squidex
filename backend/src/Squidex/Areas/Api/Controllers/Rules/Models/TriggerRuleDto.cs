// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public class TriggerRuleDto
{
    /// <summary>
    /// The optional value to send to the flow.
    /// </summary>
    public JsonValue Value { get; set; }

    public TriggerRule ToCommand(DomainId id)
    {
        return SimpleMapper.Map(this, new TriggerRule { RuleId = id });
    }
}
