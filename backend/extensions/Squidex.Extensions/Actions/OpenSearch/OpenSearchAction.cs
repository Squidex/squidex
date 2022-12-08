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

namespace Squidex.Extensions.Actions.OpenSearch;

[RuleAction(
    Title = "OpenSearch",
    IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 64 64'><path d='M61.737 23.5a2.263 2.263 0 0 0-2.262 2.263c0 18.618-15.094 33.712-33.712 33.712a2.263 2.263 0 1 0 0 4.525C46.88 64 64 46.88 64 25.763a2.263 2.263 0 0 0-2.263-2.263Z' fill='#fff'/><path d='M48.081 38c2.176-3.55 4.28-8.282 3.866-14.908C51.09 9.367 38.66-1.045 26.921.084c-4.596.441-9.314 4.187-8.895 10.896.182 2.916 1.61 4.637 3.928 5.96 2.208 1.26 5.044 2.057 8.259 2.961 3.883 1.092 8.388 2.32 11.85 4.87 4.15 3.058 6.986 6.603 6.018 13.229Z' fill='#fff'/><path d='M3.919 14C1.743 17.55-.361 22.282.052 28.908.91 42.633 13.342 53.045 25.08 51.916c4.596-.441 9.314-4.187 8.895-10.896-.182-2.916-1.61-4.637-3.928-5.96-2.208-1.26-5.044-2.057-8.259-2.961-3.883-1.092-8.388-2.32-11.85-4.87C5.787 24.17 2.95 20.625 3.919 14Z' fill='#fff'/></svg>",
    IconColor = "#005EB8",
    Display = "Populate OpenSearch index",
    Description = "Populate a full text search index in OpenSearch.",
    ReadMore = "https://opensearch.org/")]
public sealed record OpenSearchAction : RuleAction
{
    [AbsoluteUrl]
    [LocalizedRequired]
    [Display(Name = "Server Url", Description = "The url to the instance or cluster.")]
    [Editor(RuleFieldEditor.Url)]
    public Uri Host { get; set; }

    [LocalizedRequired]
    [Display(Name = "Index Name", Description = "The name of the index.")]
    [Editor(RuleFieldEditor.Text)]
    [Formattable]
    public string IndexName { get; set; }

    [Display(Name = "Username", Description = "The optional username.")]
    [Editor(RuleFieldEditor.Text)]
    public string Username { get; set; }

    [Display(Name = "Password", Description = "The optional password.")]
    [Editor(RuleFieldEditor.Text)]
    public string Password { get; set; }

    [Display(Name = "Document", Description = "The optional custom document.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the document.")]
    [Editor(RuleFieldEditor.Text)]
    public string Delete { get; set; }
}
