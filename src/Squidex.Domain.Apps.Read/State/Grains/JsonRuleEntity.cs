// ==========================================================================
//  JsonRuleEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed class JsonRuleEntity :
        JsonEntity<JsonRuleEntity>,
        IRuleEntity,
        IUpdateableEntityWithAppRef,
        IUpdateableEntityWithCreatedBy,
        IUpdateableEntityWithLastModifiedBy
    {
        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public RefToken CreatedBy { get; set; }

        [JsonProperty]
        public RefToken LastModifiedBy { get; set; }

        [JsonProperty]
        public Rule RuleDef { get; set; }
    }
}
