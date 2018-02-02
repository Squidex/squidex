// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Squidex.Domain.Apps.Core.Rules
{
    public interface IRuleTriggerVisitor<out T>
    {
        T Visit(AssetChangedTrigger trigger);

        T Visit(ContentChangedTrigger trigger);
    }
}
