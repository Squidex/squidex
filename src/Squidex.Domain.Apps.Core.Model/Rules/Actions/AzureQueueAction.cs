// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    [TypeName(nameof(AzureQueueAction))]
    public sealed class AzureQueueAction : RuleAction
    {
        private string connectionString;
        private string queue;

        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                ThrowIfFrozen();

                connectionString = value;
            }
        }

        public string Queue
        {
            get
            {
                return queue;
            }
            set
            {
                ThrowIfFrozen();

                queue = value;
            }
        }

        public override T Accept<T>(IRuleActionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
