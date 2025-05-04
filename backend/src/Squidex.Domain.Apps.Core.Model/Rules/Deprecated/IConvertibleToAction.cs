// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules.Deprecated;

[Obsolete("Has been replaced by flows.")]
public interface IConvertibleToAction
{
    RuleAction ToAction();
}
