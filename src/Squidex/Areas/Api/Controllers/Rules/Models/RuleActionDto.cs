// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Squidex.Areas.Api.Controllers.Rules.Models.Actions;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    [JsonConverter(typeof(JsonInheritanceConverter), "actionType")]
    [KnownType(typeof(AlgoliaActionDto))]
    [KnownType(typeof(WebhookActionDto))]
    public abstract class RuleActionDto
    {
        public abstract RuleAction ToAction();
    }
}
