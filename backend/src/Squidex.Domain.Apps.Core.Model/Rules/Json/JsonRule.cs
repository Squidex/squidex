// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Json
{
    public sealed class JsonRule
    {
        [JsonProperty]
        public RuleTrigger Trigger { get; set; }

        [JsonProperty]
        public RuleAction Action { get; set; }

        [JsonProperty]
        public bool IsEnabled { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        public JsonRule()
        {
        }

        public JsonRule(Rule rule)
        {
            SimpleMapper.Map(rule, this);
        }

        public Rule ToRule()
        {
            var trigger = Trigger;

            if (trigger is IMigrated<RuleTrigger> migrated)
            {
                trigger = migrated.Migrate();
            }

            var rule = new Rule(trigger, Action);

            if (!IsEnabled)
            {
                rule = rule.Disable();
            }

            if (Name != null)
            {
                rule = rule.Rename(Name);
            }

            return rule;
        }
    }
}
