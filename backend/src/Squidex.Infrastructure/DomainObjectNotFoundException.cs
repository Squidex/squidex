// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectNotFoundException(string id, Exception? inner = null) : DomainObjectException(FormatMessage(id), id, ExposedErrorCode, inner)
{
    private const string ExposedErrorCode = "OBJECT_NOTFOUND";

    private static string FormatMessage(string id)
    {
        return T.Get("exceptions.domainObjectNotFound", new { id });
    }
}
