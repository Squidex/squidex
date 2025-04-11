// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules.Deprecated;

[Obsolete("Use Flows")]
public interface IConvertibleToAction
{
    RuleAction ToAction();
}
