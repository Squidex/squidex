// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Frontend.Middlewares;

public sealed class OptionsFeature
{
    public Dictionary<string, object> Options { get; } = new Dictionary<string, object>();
}
