// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Squidex.Domain.Apps.Core.Rules
{
    public interface IRuleTriggerVisitor<out T>
    {
        T Visit(AssetChangedTriggerV2 trigger);

        T Visit(ContentChangedTriggerV2 trigger);

        T Visit(CommentTrigger trigger);

        T Visit(ManualTrigger trigger);

        T Visit(SchemaChangedTrigger trigger);

        T Visit(UsageTrigger trigger);
    }
}
