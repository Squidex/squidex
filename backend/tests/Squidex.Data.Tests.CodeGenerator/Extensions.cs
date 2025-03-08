// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex;

public static class Extensions
{
    private const char NullChar = (char)0;

    public static string ToPascalCase(this string value)
    {
        return value.AsSpan().ToPascalCase();
    }

    public static string ToPascalCase(this ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length);

        var last = NullChar;
        var length = 0;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '-' || c == '_' || c == ' ')
            {
                if (last != NullChar)
                {
                    sb.Append(char.ToUpperInvariant(last));
                }

                last = NullChar;
                length = 0;
            }
            else
            {
                if (length > 1)
                {
                    sb.Append(c);
                }
                else if (length == 0)
                {
                    last = c;
                }
                else
                {
                    sb.Append(char.ToUpperInvariant(last));
                    sb.Append(c);

                    last = NullChar;
                }

                length++;
            }
        }

        if (last != NullChar)
        {
            sb.Append(char.ToUpperInvariant(last));
        }

        return sb.ToString();
    }
}
