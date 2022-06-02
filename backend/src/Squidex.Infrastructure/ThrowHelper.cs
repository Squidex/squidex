// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar

namespace Squidex.Infrastructure
{
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

        public static void NotImplementedException(string? message = null)
        {
            throw new NotImplementedException(message);
        }

        public static void NotSupportedException(string? message = null)
        {
            throw new NotSupportedException(message);
        }
    }
}
