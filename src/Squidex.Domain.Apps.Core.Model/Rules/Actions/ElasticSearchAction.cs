// ==========================================================================
//  ElasticSearchAction.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(ElasticSearchAction))]
    public class ElasticSearchAction : RuleAction
    {
        private string indexName;
        private string typeNameForSchema;
        private string hostUrl;
        private bool requiresAuthentication;
        private string username;
        private string password;

        /// <summary>
        /// The url of elastic search. e.g. http://localhost:9200
        /// </summary>
        public string HostUrl { get; set; }

        /// <summary>
        /// The name of the type for the elasticsearch mappings of the schema.
        /// </summary>
        public string TypeNameForSchema { get; set; }

        /// <summary>
        /// The name of the index this action will operate on.
        /// </summary>
        public string IndexName { get; set; }

        public bool RequiresAuthentication { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}