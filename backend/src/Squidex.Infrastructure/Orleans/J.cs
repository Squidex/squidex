// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Json;

#pragma warning disable SA1401 // Fields must be private
#pragma warning disable CA2211 // Non-constant fields should not be visible

namespace Squidex.Infrastructure.Orleans
{
    public static class J
    {
        public static IJsonSerializer DefaultSerializer;

        public static J<T> AsJ<T>(this T value)
        {
            return new J<T>(value);
        }

        public static J<T> Of<T>(T value)
        {
            return value;
        }

        public static Task<J<T>> AsTask<T>(T value)
        {
            return Task.FromResult<J<T>>(value);
        }
    }
}
