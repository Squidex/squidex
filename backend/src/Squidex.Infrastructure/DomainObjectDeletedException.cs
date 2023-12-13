// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectDeletedException : DomainObjectException
{
    private const string ExposedErrorCode = "OBJECT_DELETED";

    public DomainObjectDeletedException(string id, Exception? inner = null)
        : base(FormatMessage(id), id, ExposedErrorCode, inner)
    {
    }

    private static string FormatMessage(string id)
    {
        return T.Get("exceptions.domainObjectDeleted", new { id });
    }
}
