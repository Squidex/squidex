// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectDeletedException(string id, Exception? inner = null) : DomainObjectException(FormatMessage(id), id, ExposedErrorCode, inner)
{
    private const string ExposedErrorCode = "OBJECT_DELETED";

    private static string FormatMessage(string id)
    {
        return T.Get("exceptions.domainObjectDeleted", new { id });
    }
}
