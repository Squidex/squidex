// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectVersionException : DomainObjectException
{
    private const string ExposedErrorCode = "OBJECT_VERSION_CONFLICT";

    public long VersionCurrent { get; }

    public long VersionExpected { get; }

    public DomainObjectVersionException(string id, long versionCurrent, long versionExpected, Exception? inner = null)
        : base(FormatMessage(id, versionCurrent, versionExpected), id, ExposedErrorCode, inner)
    {
        VersionCurrent = versionCurrent;
        VersionExpected = versionExpected;
    }

    private static string FormatMessage(string id, long currentVersion, long expectedVersion)
    {
        return T.Get("exceptions.domainObjectVersion", new { id, currentVersion, expectedVersion });
    }
}
