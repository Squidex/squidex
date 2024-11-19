// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectVersionException(string id, long versionCurrent, long versionExpected, Exception? inner = null) : DomainObjectException(FormatMessage(id, versionCurrent, versionExpected), id, ExposedErrorCode, inner)
{
    private const string ExposedErrorCode = "OBJECT_VERSION_CONFLICT";

    public long VersionCurrent { get; } = versionCurrent;

    public long VersionExpected { get; } = versionExpected;

    private static string FormatMessage(string id, long currentVersion, long expectedVersion)
    {
        return T.Get("exceptions.domainObjectVersion", new { id, currentVersion, expectedVersion });
    }
}
