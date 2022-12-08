// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class QueryParser
{
    private readonly Func<string, string> fieldProvider;

    public QueryParser(Func<string, string> fieldProvider)
    {
        this.fieldProvider = fieldProvider;
    }

    public Query? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = text.Trim();
        text = ConvertFieldNames(text);

        return new Query
        {
            Text = text
        };
    }

    private string ConvertFieldNames(string query)
    {
        var indexOfColon = query.IndexOf(':', StringComparison.Ordinal);

        if (indexOfColon < 0)
        {
            return query;
        }

        var sb = new StringBuilder();

        int position = 0, lastIndexOfColon = 0;

        while (indexOfColon >= 0)
        {
            lastIndexOfColon = indexOfColon;

            int i;
            for (i = indexOfColon - 1; i >= position; i--)
            {
                var c = query[i];

                if (!char.IsLetter(c) && c != '-' && c != '_')
                {
                    break;
                }
            }

            i++;

            sb.Append(query[position..i]);
            sb.Append(fieldProvider(query[i..indexOfColon]));

            position = indexOfColon + 1;

            indexOfColon = query.IndexOf(':', position);
        }

        sb.Append(query[lastIndexOfColon..]);

        return sb.ToString();
    }
}
