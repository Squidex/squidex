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

namespace Squidex.Extensions.Actions.ElasticSearch;

public sealed record ElasticSearchAction : RuleAction
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
    public string? Username { get; set; }

    [Display(Name = "Password", Description = "The optional password.")]
    [Editor(RuleFieldEditor.Text)]
    public string? Password { get; set; }

    [Display(Name = "Document", Description = "The optional custom document.")]
    [Editor(RuleFieldEditor.TextArea)]
    [Formattable]
    public string? Document { get; set; }

    [Display(Name = "Deletion", Description = "The condition when to delete the document.")]
    [Editor(RuleFieldEditor.Text)]
    public string? Delete { get; set; }
}
