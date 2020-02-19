// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
    {
        public static Dictionary<string, string> ToTexts(this NamedContentData data)
        {
            var result = new Dictionary<string, string>();

            if (data != null)
            {
                var languages = new Dictionary<string, StringBuilder>();

                void AppendText(string language, string text)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var sb = languages.GetOrAddNew(language);

                        if (sb.Length > 0)
                        {
                            sb.Append(" ");
                        }

                        sb.Append(text);
                    }
                }

                foreach (var value in data.Values)
                {
                    if (value != null)
                    {
                        foreach (var (key, jsonValue) in value)
                        {
                            var appendText = new Action<string>(text => AppendText(key, text));

                            AppendJsonText(jsonValue, appendText);
                        }
                    }
                }

                foreach (var (key, value) in languages)
                {
                    result[key] = value.ToString();
                }
            }

            return result;
        }

        private static void AppendJsonText(IJsonValue value, Action<string> appendText)
        {
            if (value.Type == JsonValueType.String)
            {
                appendText(value.ToString());
            }
            else if (value is JsonArray array)
            {
                foreach (var item in array)
                {
                    AppendJsonText(item, appendText);
                }
            }
            else if (value is JsonObject obj)
            {
                foreach (var item in obj.Values)
                {
                    AppendJsonText(item, appendText);
                }
            }
        }
    }
}
