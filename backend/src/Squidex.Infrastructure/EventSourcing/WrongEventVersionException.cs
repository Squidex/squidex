// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

[Serializable]
public class WrongEventVersionException : Exception
{
    public long VersionCurrent { get; }

    public long VersionExpected { get; }

    public WrongEventVersionException(long versionCurrent, long versionExpected, Exception? inner = null)
        : base(FormatMessage(versionCurrent, versionExpected), inner)
    {
        VersionCurrent = versionCurrent;
        VersionExpected = versionExpected;
    }

    private static string FormatMessage(long currentVersion, long expectedVersion)
    {
        return $"Requested version {expectedVersion}, but found {currentVersion}.";
    }
}
