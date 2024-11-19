// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

[Serializable]
public class WrongEventVersionException(long versionCurrent, long versionExpected, Exception? inner = null) : Exception(FormatMessage(versionCurrent, versionExpected), inner)
{
    public long VersionCurrent { get; } = versionCurrent;

    public long VersionExpected { get; } = versionExpected;

    private static string FormatMessage(long currentVersion, long expectedVersion)
    {
        return $"Requested version {expectedVersion}, but found {currentVersion}.";
    }
}
