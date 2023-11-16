// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

[Serializable]
public class DomainObjectException : DomainException
{
    public string Id { get; }

    public DomainObjectException(string message, string id, string errorCode, Exception? inner = null)
        : base(message, errorCode, inner)
    {
        Guard.NotNullOrEmpty(id);

        Id = id;
    }
}
