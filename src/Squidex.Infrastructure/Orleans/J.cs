// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Orleans
{
    public static class J
    {
        internal static readonly JsonSerializer DefaultSerializer = JsonSerializer.CreateDefault();

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
