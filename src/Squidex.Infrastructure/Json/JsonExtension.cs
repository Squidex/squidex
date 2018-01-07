// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
