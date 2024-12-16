// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex.Extensions.Actions.Twitter;

public sealed record TweetAction : RuleAction<TweetStep>
{
}
