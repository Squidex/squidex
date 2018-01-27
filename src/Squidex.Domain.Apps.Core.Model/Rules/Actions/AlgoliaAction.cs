// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(AlgoliaAction))]
    public sealed class AlgoliaAction : RuleAction
    {
        private string appId;
        private string apiKey;
        private string indexName;

        public string AppId
        {
            get
            {
                return appId;
            }
            set
            {
                ThrowIfFrozen();

                appId = value;
            }
        }

        public string ApiKey
        {
            get
            {
                return apiKey;
            }
            set
            {
                ThrowIfFrozen();

                apiKey = value;
            }
        }

        public string IndexName
        {
            get
            {
                return indexName;
            }
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
    }
}
