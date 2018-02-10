// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "triggerType", typeof(RuleTriggerDto))]
    [KnownType(nameof(Subtypes))]
    public abstract class RuleTriggerDto
    {
        public abstract RuleTrigger ToTrigger();

        public static Type[] Subtypes()
        {
            var type = typeof(RuleTriggerDto);

            return type.Assembly.GetTypes().Where(type.IsAssignableFrom).ToArray();
        }
    }
}
