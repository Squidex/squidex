// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States;

[Serializable]
public class InconsistentStateException(long current, long expected, Exception? inner = null) : Exception(FormatMessage(current, expected), inner)
{
    public long VersionCurrent { get; } = current;

    public long VersionExpected { get; } = expected;

    private static string FormatMessage(long current, long expected)
    {
        return $"Requested version {expected}, but found {current}.";
    }
}
