// ==========================================================================
//  CustomJsonSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Config.Domain;
using Squidex.Infrastructure.Json.Orleans;

namespace Squidex.Config.Orleans
{
    public class CustomJsonSerializer : JsonExternalSerializer
    {
        public CustomJsonSerializer()
            : base(JsonSerializer.Create(SerializationServices.DefaultJsonSettings))
        {
        }
    }
}
