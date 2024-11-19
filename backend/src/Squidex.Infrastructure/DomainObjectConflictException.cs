// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectConflictException(string id, Exception? inner = null) : DomainObjectException(FormatMessage(id), id, ExposedErrorCode, inner)
{
    private const string ExposedErrorCode = "OBJECT_CONFLICT";

    private static string FormatMessage(string id)
    {
        return T.Get("exceptions.domainObjectConflict", new { id });
    }
}
