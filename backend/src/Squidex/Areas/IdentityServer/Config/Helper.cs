// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Areas.IdentityServer.Config
{
    public class Helper
    {
        public static string BuildId(string value)
        {
            const int MongoDbLength = 24;

            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(value);

            foreach (var c in bytes)
            {
                sb.Append(c.ToString("X2"));

                if (sb.Length == MongoDbLength)
                {
                    break;
                }
            }

            while (sb.Length < MongoDbLength)
            {
                sb.Append('0');
            }

            return sb.ToString();
        }
    }
}
