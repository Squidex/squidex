// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectConflictException : DomainObjectException
{
    private const string ExposedErrorCode = "OBJECT_CONFLICT";

    public DomainObjectConflictException(string id, Exception? inner = null)
        : base(FormatMessage(id), id, ExposedErrorCode, inner)
    {
    }

    protected DomainObjectConflictException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    private static string FormatMessage(string id)
    {
        return T.Get("exceptions.domainObjectConflict", new { id });
    }
}
