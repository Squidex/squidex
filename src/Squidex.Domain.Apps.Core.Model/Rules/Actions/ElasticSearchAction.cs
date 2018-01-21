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
        private Uri addressOfElasticSearch;
        private string indexName;
        private string typeNameForSchema;

        public ElasticSearchAction()
        {
            Validator = new ElasticSearchActionValidator();
        }

        /// <summary>
        /// The name of the type for the elasticsearch mappings of the schema.
        /// </summary>
        public string TypeNameForSchema
        {
            get => typeNameForSchema;
            set
            {
                ThrowIfFrozen();
                typeNameForSchema = value;
            }
        }

        /// <summary>
        /// The name of the index this action will operate on.
        /// </summary>
        public string IndexName
        {
            get => indexName;
            set
            {
                ThrowIfFrozen();
                indexName = value;
            }
        }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override IRuleActionValidator Validator { get; }
    }
}