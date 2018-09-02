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
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Rules.Action.ElasticSearch
{
    [RuleActionHandler(typeof(ElasticSearchActionHandler))]
    [RuleAction(Link = "https://www.elastic.co/",
        Display = "Populate ElasticSearch index",
        Description = "Populate and synchronize indices in ElasticSearch for full text search.")]
    public sealed class ElasticSearchAction : RuleAction
    {
        [AbsoluteUrl]
        [Required]
        [Display(Name = "Host", Description = "The hostname of the elastic search instance or cluster.")]
        public Uri Host { get; set; }

        [Required]
        [Display(Name = "Index Name", Description = "The name of the index.")]
        public string IndexName { get; set; }

        [Required]
        [Display(Name = "Index Type", Description = "The name of the index type.")]
        public string IndexType { get; set; }

        [Display(Name = "Username", Description = "The optional username.")]
        public string Username { get; set; }

        [Display(Name = "Password", Description = "The optional password.")]
        public string Password { get; set; }
    }
}
