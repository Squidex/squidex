// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.MongoDb
{
    public static class BsonHelper
    {
        private const string TypeBson = "§type";
        private const string TypeJson = "$type";

        public static string UnescapeBson(this string value)
        {
            if (value == TypeBson)
            {
                return TypeJson;
            }

            return ReplaceFirstCharacter(value, '§', '$');
        }

        public static string EscapeJson(this string value)
        {
            if (value == TypeJson)
            {
                return TypeBson;
            }

            return ReplaceFirstCharacter(value, '$', '§');
        }

        private static string ReplaceFirstCharacter(string value, char toReplace, char replacement)
        {
            if (value.Length == 0 || value[0] != toReplace)
            {
                return value;
            }

            if (value.Length == 1)
            {
                return toReplace.ToString();
            }

            return replacement + value[1..];
        }
    }
}
