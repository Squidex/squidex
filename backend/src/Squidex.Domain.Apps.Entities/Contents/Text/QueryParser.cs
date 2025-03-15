// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class QueryParser(Func<string, string> fieldProvider)
{
    public Query? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = ConvertFieldNames(text);

        return new Query
        {
            Text = text,
        };
    }

    private string ConvertFieldNames(string query)
    {
        var indexOfColon = query.IndexOf(':', StringComparison.Ordinal);
        if (indexOfColon < 0)
        {
            return query.Trim();
        }

        var span = query.AsSpan().Trim();

        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            while (indexOfColon >= 0)
            {
                int i;
                for (i = indexOfColon - 1; i >= 0; i--)
                {
                    var c = span[i];
                    if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    {
                        break;
                    }
                }

                i++;

                var fieldName = span[i..indexOfColon].ToString();

                sb.Append(span[..i]);
                sb.Append(fieldProvider(fieldName));
                sb.Append(':');

                span = span[(indexOfColon + 1)..];

                indexOfColon = span.IndexOf(':');
            }

            sb.Append(span);

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }
}
