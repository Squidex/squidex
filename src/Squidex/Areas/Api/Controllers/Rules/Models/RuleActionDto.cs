// ==========================================================================
//  RuleActionDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Areas.Api.Controllers.Rules.Models.Actions;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "actionType")]
    [KnownType(typeof(WebhookActionDto))]
    public abstract class RuleActionDto
    {
        public abstract RuleAction ToAction();
    }
}
