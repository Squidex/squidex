// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure;

public static class ThrowHelper
{
    public static void ArgumentException(string message, string? paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    public static void ArgumentNullException(string? paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    public static void KeyNotFoundException(string? message = null)
    {
        throw new KeyNotFoundException(message);
    }

    public static void InvalidOperationException(string? message = null)
    {
        throw new InvalidOperationException(message);
    }

    public static void InvalidCastException(string? message = null)
    {
        throw new InvalidCastException(message);
    }

    public static void JsonException(string? message = null, Exception? ex = null)
    {
        throw new JsonException(message, ex);
    }

    public static void NotSupportedException(string? message = null)
    {
        throw new NotSupportedException(message);
    }
}
