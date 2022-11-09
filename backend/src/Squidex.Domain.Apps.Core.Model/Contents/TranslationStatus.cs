// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents;

public class TranslationStatus : Dictionary<string, int>
{
    public TranslationStatus()
    {
    }

    public TranslationStatus(int capacity)
        : base(capacity)
    {
    }

    public static TranslationStatus Create(ContentData data, Schema schema, LanguagesConfig languages)
    {
        Guard.NotNull(data);
        Guard.NotNull(schema);
        Guard.NotNull(languages);

        var result = new TranslationStatus(languages.Languages.Count);

        var localizedFields = schema.Fields.Where(x => x.Partitioning == Partitioning.Language).ToList();

        foreach (var language in languages.AllKeys)
        {
            var percent = 0;

            foreach (var field in localizedFields)
            {
                if (IsValidValue(data.GetValueOrDefault(field.Name)?.GetValueOrDefault(language)))
                {
                    percent++;
                }
            }

            if (localizedFields.Count > 0)
            {
                percent = (int)Math.Round(100 * (double)percent / localizedFields.Count);
            }
            else
            {
                percent = 100;
            }

            result[language] = percent;
        }

        return result;
    }

    private static bool IsValidValue(JsonValue? value)
    {
        return value != null && value.Value.Type != JsonValueType.Null;
    }
}
