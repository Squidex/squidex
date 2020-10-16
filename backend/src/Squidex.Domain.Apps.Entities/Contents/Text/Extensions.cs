// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool =
            new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public static Dictionary<string, string> ToTexts(this NamedContentData data)
        {
            var result = new Dictionary<string, string>();

            if (data != null)
            {
                var languages = new Dictionary<string, StringBuilder>();
                try
                {
                    foreach (var value in data.Values)
                    {
                        if (value != null)
                        {
                            foreach (var (key, jsonValue) in value)
                            {
                                AppendJsonText(languages, key, jsonValue);
                            }
                        }
                    }

                    foreach (var (key, sb) in languages)
                    {
                        result[key] = sb.ToString();
                    }
                }
                finally
                {
                    foreach (var (_, sb) in languages)
                    {
                        StringBuilderPool.Return(sb);
                    }
                }
            }

            return result;
        }

        private static void AppendJsonText(Dictionary<string, StringBuilder> languages, string language, IJsonValue value)
        {
            if (value.Type == JsonValueType.String)
            {
                AppendText(languages, language, value.ToString());
            }
            else if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    AppendJsonText(languages, language, item);
                }
            }
            else if (value is JsonObject obj)
            {
                foreach (var item in obj.Values)
                {
                    AppendJsonText(languages, language, item);
                }
            }
        }

        private static void AppendText(Dictionary<string, StringBuilder> languages, string language, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!languages.TryGetValue(language, out var sb))
                {
                    sb = StringBuilderPool.Get();

                    languages[language] = sb;
                }

                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                foreach (var c in text)
                {
                    if (IsCJKLetter(c))
                    {
                        sb.Append(c);
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
        }

        private static bool IsCJKLetter(char c)
        {
            return char.IsLetter(c) && char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter && !IsKatakana(c);
        }

        private static bool IsKatakana(char c)
        {
            return c >= '\u30A0' && c <= '\u30FF';
        }
    }
}
