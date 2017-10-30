// ==========================================================================
//  RuleTriggerDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Runtime.Serialization;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using Squidex.Controllers.Api.Rules.Models.Triggers;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Controllers.Api.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "triggerType")]
    [KnownType(typeof(ContentChangedTriggerDto))]
    public abstract class RuleTriggerDto
    {
        public abstract RuleTrigger ToTrigger();
    }
}
