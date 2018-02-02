// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "actionType", typeof(RuleActionDto))]
    public abstract class RuleActionDto
    {
        public abstract RuleAction ToAction();

        public static Type[] Subtypes()
        {
            var type = typeof(RuleActionDto);

            return type.Assembly.GetTypes().Where(type.IsAssignableFrom).ToArray();
        }
    }
}
