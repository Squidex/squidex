// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class JintScriptOptions
{
    public TimeSpan TimeoutScript { get; set; } = TimeSpan.FromMilliseconds(200);

    public TimeSpan TimeoutExecution { get; set; } = TimeSpan.FromMilliseconds(4000);
}
