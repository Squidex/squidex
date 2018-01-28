﻿// ==========================================================================
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

        public ElasticSearchAction()
        {
            Validator = new ElasticSearchActionValidator();
        }

        /// <summary>
        /// The url of elastic search. e.g. http://localhost:9200
        /// </summary>
        public string HostUrl
        {
            get => hostUrl;
            set
            {
                ThrowIfFrozen();
                hostUrl = value;
            }
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

        public bool RequiresAuthentication
        {
            get => requiresAuthentication;
            set
            {
                ThrowIfFrozen();
                requiresAuthentication = value;
            }
        }

        public string Username
        {
            get => username;
            set
            {
                ThrowIfFrozen();
                username = value;
            }
        }

        public string Password
        {
            get => password;
            set
            {
                ThrowIfFrozen();
                password = value;
            }
        }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override IRuleActionValidator Validator { get; }
    }
}