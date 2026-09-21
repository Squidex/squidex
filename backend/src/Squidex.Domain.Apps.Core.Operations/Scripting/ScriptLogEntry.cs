// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using NodaTime;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed record ScriptLogEntry(Instant Timestamp, string Level, string Message)
{
    public override string ToString()
    {
        return $"console.{Level}: {Message}";
    }
}
