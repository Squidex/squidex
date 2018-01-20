// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Areas.Api.Controllers.Rules.Models.Triggers;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "triggerType")]
    [KnownType(typeof(ContentChangedTriggerDto))]
    public abstract class RuleTriggerDto
    {
        public abstract RuleTrigger ToTrigger();
    }
}
