// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Actions;

namespace Squidex.Domain.Apps.Core.Rules
{
    public interface IRuleActionVisitor<out T>
    {
        T Visit(AlgoliaAction action);

        T Visit(AzureQueueAction action);

        T Visit(ElasticSearchAction action);

        T Visit(FastlyAction action);

        T Visit(MediumAction action);

        T Visit(SlackAction action);

        T Visit(WebhookAction action);
    }
}
