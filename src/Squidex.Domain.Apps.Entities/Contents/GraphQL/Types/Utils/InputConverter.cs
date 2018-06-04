// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public static class InputConverter
    {
        public static Inputs ToInputs(JObject input)
        {
            var result = new Inputs();

            if (input != null)
            {
                foreach (var kvp in input)
                {
                    result.Add(kvp.Key, GetValue(kvp.Value, 1));
                }
            }

            return result;
        }

        private static object GetValue(object value, int level)
        {
            if (level == 3)
            {
                return value;
            }

            switch (value)
            {
                case JObject jObject:
                    {
                        var result = new Dictionary<string, object>();

                        foreach (var kvp in jObject)
                        {
                            result.Add(kvp.Key, GetValue(kvp.Value, level + 1));
                        }

                        return result;
                    }

                case JArray jArray:
                    {
                        var result = new List<object>();

                        foreach (var item in jArray)
                        {
                            result.Add(GetValue(item, level + 1));
                        }

                        return result;
                    }

                case JValue jValue:
                    {
                        return jValue.Value;
                    }
            }

            return value;
        }
    }
}
