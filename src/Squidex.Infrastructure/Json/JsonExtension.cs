// ==========================================================================
//  JsonExtension.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.Json
{
    public static class JsonExtension
    {
        public static bool IsNull(this JToken token)
        {
            if (token == null)
            {
                return true;
            }

            if (token.Type == JTokenType.Null)
            {
                return true;
            }

            if (token is JValue value)
            {
                return value.Value == null;
            }

            return false;
        }
    }
}
