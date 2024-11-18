// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

[Serializable]
public class DomainForbiddenException(string message, Exception? inner = null) : DomainException(message, ValidationError, inner)
{
    private const string ValidationError = "FORBIDDEN";
}
