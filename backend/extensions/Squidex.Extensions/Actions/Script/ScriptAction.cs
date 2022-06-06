// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Script
{
    [RuleAction(
        Title = "Script",
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512'><path d='M112.155 67.644h84.212v236.019c0 106.375-50.969 143.497-132.414 143.497-19.944 0-45.429-3.324-62.052-8.864l9.419-68.146c11.635 3.878 26.594 6.648 43.214 6.648 35.458 0 57.621-16.068 57.621-73.687V67.644zM269.484 354.634c22.161 11.635 57.62 23.27 93.632 23.27 38.783 0 59.282-16.066 59.282-40.998 0-22.715-17.729-36.565-62.606-52.079-62.053-22.162-103.05-56.512-103.05-111.36 0-63.715 53.741-111.917 141.278-111.917 42.662 0 73.132 8.313 95.295 18.838l-18.839 67.592c-14.404-7.201-41.553-17.729-77.562-17.729-36.567 0-54.297 17.175-54.297 36.013 0 23.824 20.499 34.349 69.256 53.188 65.928 24.378 96.4 58.728 96.4 111.915 0 62.606-47.647 115.794-150.143 115.794-42.662 0-84.77-11.636-105.82-23.27l17.174-69.257z'/></svg>",
        IconColor = "#f0be25",
        Display = "Run a Script",
        Description = "Runs a custom Javascript")]
    public sealed record ScriptAction : RuleAction
    {
        [LocalizedRequired]
        [Display(Name = "Script", Description = "The script to render.")]
        [Editor(RuleFieldEditor.Javascript)]
        [Formattable]
        public string Script { get; set; }
    }
}
