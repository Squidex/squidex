// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.ElasticSearch
{
    [RuleAction(
        Title = "Elasticsearch",
        IconImage = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 29 28'><path d='M13.427 17.436H4.163C3.827 16.354 3.636 15.2 3.636 14s.182-2.355.527-3.436h15.245c1.891 0 3.418 1.545 3.418 3.445a3.421 3.421 0 0 1-3.418 3.427h-5.982zm-.436 1.146H4.6a11.508 11.508 0 0 0 4.2 4.982 11.443 11.443 0 0 0 15.827-3.209 5.793 5.793 0 0 0-4.173-1.773H12.99zm7.464-9.164a5.794 5.794 0 0 0 4.173-1.773 11.45 11.45 0 0 0-9.536-5.1c-2.327 0-4.491.7-6.3 1.891a11.554 11.554 0 0 0-4.2 4.982h15.864z'/></svg>",
        IconColor = "#1e5470",
        Display = "Populate Elasticsearch index",
        Description = "Populate a full text search index in ElasticSearch.",
        ReadMore = "https://www.elastic.co/")]
    public sealed record ElasticSearchAction : RuleAction
    {
        [AbsoluteUrl]
        [LocalizedRequired]
        [Display(Name = "Server Url", Description = "The url to the elastic search instance or cluster.")]
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
}
