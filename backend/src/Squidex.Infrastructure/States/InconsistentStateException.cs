// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States;

[Serializable]
public class InconsistentStateException : Exception
{
    public long VersionCurrent { get; }

    public long VersionExpected { get; }

    public InconsistentStateException(long current, long expected, Exception? inner = null)
        : base(FormatMessage(current, expected), inner)
    {
        VersionCurrent = current;
        VersionExpected = expected;
    }

    private static string FormatMessage(long current, long expected)
    {
        return $"Requested version {expected}, but found {current}.";
    }
}
