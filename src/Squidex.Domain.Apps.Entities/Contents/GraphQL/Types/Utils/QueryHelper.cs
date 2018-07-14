// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public static class QueryHelper
    {
        public static string ToOData(this Dictionary<string, object> arguments, HashSet<string> fieldsToSkip = null)
        {
            var sb = new StringBuilder("?");

            foreach (var kvp in arguments)
            {
                if (kvp.Value != null && (fieldsToSkip == null || !fieldsToSkip.Contains(kvp.Key)))
                {
                    var value = string.Format(CultureInfo.InvariantCulture, "{0}", kvp.Value);

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (sb.Length > 1)
                        {
                            sb.Append("&");
                        }

                        sb.Append("$");
                        sb.Append(kvp.Key);
                        sb.Append("=");
                        sb.Append(value);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
