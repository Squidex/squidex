// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions
{
    public static class TriggerTypes
    {
        private const string TriggerSuffix = "Trigger";
        private const string TriggerSuffixV2 = "TriggerV2";

        public static readonly IReadOnlyDictionary<string, RuleElement> All = new Dictionary<string, RuleElement>
        {
            [GetTriggerName(typeof(ContentChangedTriggerV2))] = new RuleElement
            {
                IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 28'><path d='M21.875 28H6.125A6.087 6.087 0 0 1 0 21.875V6.125A6.087 6.087 0 0 1 6.125 0h15.75A6.087 6.087 0 0 1 28 6.125v15.75A6.088 6.088 0 0 1 21.875 28zM6.125 1.75A4.333 4.333 0 0 0 1.75 6.125v15.75a4.333 4.333 0 0 0 4.375 4.375h15.75a4.333 4.333 0 0 0 4.375-4.375V6.125a4.333 4.333 0 0 0-4.375-4.375H6.125z'/><path d='M13.125 12.25H7.35c-1.575 0-2.888-1.313-2.888-2.888V7.349c0-1.575 1.313-2.888 2.888-2.888h5.775c1.575 0 2.887 1.313 2.887 2.888v2.013c0 1.575-1.312 2.888-2.887 2.888zM7.35 6.212c-.613 0-1.138.525-1.138 1.138v2.012A1.16 1.16 0 0 0 7.35 10.5h5.775a1.16 1.16 0 0 0 1.138-1.138V7.349a1.16 1.16 0 0 0-1.138-1.138H7.35zM22.662 16.713H5.337c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h17.237c.525 0 .875.35.875.875s-.35.875-.787.875zM15.138 21.262h-9.8c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h9.713c.525 0 .875.35.875.875s-.35.875-.787.875z'/></svg>",
                IconColor = "#3389ff",
                Display = "Content changed",
                Description = "For content changes like created, updated, published, unpublished..."
            },
            [GetTriggerName(typeof(AssetChangedTriggerV2))] = new RuleElement
            {
                IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 28'><path d='M21.875 28H6.125A6.087 6.087 0 0 1 0 21.875V6.125A6.087 6.087 0 0 1 6.125 0h15.75A6.087 6.087 0 0 1 28 6.125v15.75A6.088 6.088 0 0 1 21.875 28zM6.125 1.75A4.333 4.333 0 0 0 1.75 6.125v15.75a4.333 4.333 0 0 0 4.375 4.375h15.75a4.333 4.333 0 0 0 4.375-4.375V6.125a4.333 4.333 0 0 0-4.375-4.375H6.125z'/><path d='M21.088 23.537H9.1c-.35 0-.612-.175-.787-.525s-.088-.7.088-.962l8.225-9.713c.175-.175.438-.35.7-.35s.525.175.7.35l5.25 7.525c.088.087.088.175.088.262.438 1.225.087 2.012-.175 2.45-.613.875-1.925.963-2.1.963zm-10.063-1.75h10.15c.175 0 .612-.088.7-.262.088-.088.088-.35 0-.7l-4.55-6.475-6.3 7.438zM9.1 13.737c-2.1 0-3.85-1.75-3.85-3.85S7 6.037 9.1 6.037s3.85 1.75 3.85 3.85-1.663 3.85-3.85 3.85zm0-5.949c-1.138 0-2.1.875-2.1 2.1s.962 2.1 2.1 2.1 2.1-.962 2.1-2.1-.875-2.1-2.1-2.1z'/></svg>",
                IconColor = "#3389ff",
                Display = "Asset changed",
                Description = "For asset changes like uploaded, updated (reuploaded), renamed, deleted..."
            },
            [GetTriggerName(typeof(SchemaChangedTrigger))] = new RuleElement
            {
                IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M9.6 0c-.6 0-1 .4-1 1s.4 1 1 1h12.8c.6 0 1.1-.4 1.1-1s-.4-1-1-1H9.6zM6.1 4.3c-.6 0-1 .4-1 1s.4 1 1 1h19.8c.5 0 .9-.4.9-1s-.4-1-1-1H6.1zM7 8.6c-3.9 0-7 3.1-7 7V25c0 3.9 3.1 7 7 7h18c3.9 0 7-3.1 7-7v-9.4c0-3.9-3.1-7-7-7H7zm0 2h18c2.8 0 5 2.2 5 5V25c0 2.8-2.2 5-5 5H7c-2.8 0-5-2.2-5-5v-9.4c0-2.8 2.2-5 5-5zM5.3 13v2c0 2.4 2 4.4 4.4 4.4h12.7c2.4 0 4.4-2 4.4-4.4v-2H25v2c0 1.5-1.2 2.6-2.6 2.6H9.6C8.2 17.7 7 16.5 7 15v-2H5.3z' id='path5869'/></svg>",
                IconColor = "#3389ff",
                Display = "Schema changed",
                Description = "When a schema definition has been created, updated, published or deleted..."
            },
            [GetTriggerName(typeof(UsageTrigger))] = new RuleElement
            {
                IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M21.2 11.4c-.2 0-.4-.1-.6-.2-.5-.3-.6-.9-.3-1.4L22 7.2c.3-.5.9-.6 1.4-.3.6.4.7 1.1.4 1.5L22.1 11c-.2.3-.5.4-.9.4zM16 20.9h-.2c-1-.1-2-.6-2.5-1.5l-6-8.7c-.3-.3-.3-.8 0-1.2.3-.3.8-.4 1.2-.2l9.2 5.4c.9.5 1.5 1.4 1.6 2.4.1 1-.2 2-.9 2.8-.6.7-1.5 1-2.4 1zm-4.6-7.5l3.4 5c.2.3.6.6 1 .6s.8-.1 1.1-.4c.3-.3.4-.7.3-1.1-.1-.4-.3-.7-.6-1zM25.9 32H6.1C2.8 32 0 29.2 0 25.9v-10C0 7.1 7.1 0 15.8 0 24.8 0 32 7.2 32 16.2v9.7c0 3.3-2.8 6.1-6.1 6.1zM15.8 2C8.2 2 2 8.2 2 15.8v10C2 28.1 3.9 30 6.1 30h19.7c2.3 0 4.1-1.9 4.1-4.1v-9.7C30 8.4 23.6 2 15.8 2z'/></svg>",
                IconColor = "#3389ff",
                Display = "Usage exceeded",
                Description = "When monthly API calls exceed a specified limit for one time a month..."
            }
        };

        private static string GetTriggerName(Type type)
        {
            return type.TypeName(false, TriggerSuffix, TriggerSuffixV2);
        }
    }
}
