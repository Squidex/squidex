// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Newtonsoft.Json;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.Orleans
{
    public static class J
    {
        public static JsonSerializer Serializer = new JsonSerializer();

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
