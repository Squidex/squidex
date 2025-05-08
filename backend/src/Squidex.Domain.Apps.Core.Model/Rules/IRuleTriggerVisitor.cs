// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Squidex.Domain.Apps.Core.Rules;

public interface IRuleTriggerVisitor<out T, in TArgs>
{
    T Visit(AssetChangedTriggerV2 trigger, TArgs args);

    T Visit(ContentChangedTriggerV2 trigger, TArgs args);

    T Visit(CommentTrigger trigger, TArgs args);

    T Visit(ManualTrigger trigger, TArgs args);

    T Visit(SchemaChangedTrigger trigger, TArgs args);

    T Visit(UsageTrigger trigger, TArgs args);
}
