// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

[Serializable]
public class DomainException : Exception
{
    public string? ErrorCode { get; }

    public DomainException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public DomainException(string message, string? errorCode, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
