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

namespace Squidex.Extensions.Actions.Algolia;

[RuleAction(
    Title = "Algolia",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><path d='M16 .842C7.633.842.842 7.625.842 16S7.625 31.158 16 31.158c8.374 0 15.158-6.791 15.158-15.166S24.375.842 16 .842zm0 25.83c-5.898 0-10.68-4.781-10.68-10.68S10.101 5.313 16 5.313s10.68 4.781 10.68 10.679-4.781 10.68-10.68 10.68zm0-19.156v7.956c0 .233.249.388.458.279l7.055-3.663a.312.312 0 0 0 .124-.434 8.807 8.807 0 0 0-7.319-4.447z'/></svg>",
    IconColor = "#0d9bf9",
    Display = "Populate Algolia index",
    Description = "Populate a full text search index in Algolia.",
    ReadMore = "https://www.algolia.com/")]
public sealed record AlgoliaAction : RuleAction
{
    [LocalizedRequired]
    [Display(Name = "Application Id", Description = "The application ID.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string AppId { get; set; }

    [LocalizedRequired]
    [Display(Name = "Api Key", Description = "The API key to grant access to Squidex.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string ApiKey { get; set; }

    [LocalizedRequired]
    [Display(Name = "Index Name", Description = "The name of the index.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string IndexName { get; set; }

    [Display(Name = "Document", Description = "The optional custom document.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the entry.")]
    [Editor(RuleFieldEditor.Text)]
    public string Delete { get; set; }
}
