// ==========================================================================
//  RuleActionDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Controllers.Api.Rules.Models.Actions;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Controllers.Api.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "actionType")]
    [KnownType(typeof(WebhookActionDto))]
    public abstract class RuleActionDto
    {
        public abstract RuleAction ToAction();
    }
}
