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

namespace Squidex.Extensions.Actions.CreateContent
{
    [RuleAction(
        Title = "CreateContent",
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 28 28'><path d='M21.875 28H6.125A6.087 6.087 0 010 21.875V6.125A6.087 6.087 0 016.125 0h15.75A6.087 6.087 0 0128 6.125v15.75A6.088 6.088 0 0121.875 28zM6.125 1.75A4.333 4.333 0 001.75 6.125v15.75a4.333 4.333 0 004.375 4.375h15.75a4.333 4.333 0 004.375-4.375V6.125a4.333 4.333 0 00-4.375-4.375H6.125z'/><path d='M13.125 12.25H7.35c-1.575 0-2.888-1.313-2.888-2.888V7.349c0-1.575 1.313-2.888 2.888-2.888h5.775c1.575 0 2.887 1.313 2.887 2.888v2.013c0 1.575-1.312 2.888-2.887 2.888zM7.35 6.212c-.613 0-1.138.525-1.138 1.138v2.012A1.16 1.16 0 007.35 10.5h5.775a1.16 1.16 0 001.138-1.138V7.349a1.16 1.16 0 00-1.138-1.138H7.35zM22.662 16.713H5.337c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h17.237c.525 0 .875.35.875.875s-.35.875-.787.875zM15.138 21.262h-9.8c-.525 0-.875-.35-.875-.875s.35-.875.875-.875h9.713c.525 0 .875.35.875.875s-.35.875-.787.875z'/></svg>",
        IconColor = "#3389ff",
        Display = "Create content",
        Description = "Create a a new content item for any schema.")]
    public sealed record CreateContentAction : RuleAction
    {
        [LocalizedRequired]
        [Display(Name = "Data", Description = "The content data.")]
        [Editor(RuleFieldEditor.TextArea)]
        [Formattable]
        public string Data { get; set; }

        [LocalizedRequired]
        [Display(Name = "Schema", Description = "The name of the schema.")]
        [Editor(RuleFieldEditor.Text)]
        public string Schema { get; set; }

        [Display(Name = "Client", Description = "An optional client name.")]
        [Editor(RuleFieldEditor.Text)]
        public string Client { get; set; }

        [Display(Name = "Publish", Description = "Publish the content.")]
        [Editor(RuleFieldEditor.Text)]
        public bool Publish { get; set; }
    }
}
