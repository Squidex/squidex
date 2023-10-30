// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Extensions.Actions.DeepDetect;

[RuleAction(
    Title = "DeepDetect",
    IconImage = "<svg viewBox='0 0 28 28' xmlns='http://www.w3.org/2000/svg'><g style='stroke-width:1.24962' fill='none'><path fill='#ff5252' d='M13 21.92H0v-8.032h9.386V10.92h3.57v11zm-9.386-4.889v1.702H9.43v-1.702z' style='stroke-width:1.24962' transform='matrix(.78667 0 0 .81405 2.529 2.668)'/><path fill='#fff' d='M29.164 21.92h-13V14.028H25.7V5.92h3.464zm-9.536-4.804v1.673H25.7v-1.673z' style='stroke-width:1.24962' transform='matrix(.78667 0 0 .81405 2.529 2.668)'/></g></svg>",
    IconColor = "#526a75",
    Display = "Annotate image",
    Description = "Annotate an image using deep detect.")]
public sealed record DeepDetectAction : RuleAction
{
    [Display(Name = "Min Probability", Description = "The minimum probability for objects to be recognized (0 - 100).")]
    [Editor(RuleFieldEditor.Number)]
    public long MinimumProbability { get; set; }

    [Display(Name = "Max Tags", Description = "The maximum number of tags to use.")]
    [Editor(RuleFieldEditor.Number)]
    public long MaximumTags { get; set; }
}
