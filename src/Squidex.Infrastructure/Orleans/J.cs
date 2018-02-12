// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.Orleans
{
    public static class J
    {
        public static JsonSerializer Serializer = new JsonSerializer();
    }
}
