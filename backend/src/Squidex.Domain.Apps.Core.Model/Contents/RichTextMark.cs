// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;
using Squidex.Text.RichText.Model;

namespace Squidex.Domain.Apps.Core.Contents;

internal sealed class RichTextMark : IMark
{
    private JsonObject? attrs;

    public MarkType Type { get; private set; }

    public bool TryUse(JsonValue source)
    {
        Type = MarkType.Undefined;

        attrs = null;

        if (source.Value is not JsonObject obj)
        {
            return false;
        }

        var isValid = true;
        foreach (var (key, value) in obj)
        {
            switch (key)
            {
                case "type" when value.TryGetEnum<MarkType>(out var type):
                    Type = type;
                    break;
                case "attrs" when value.Value is JsonObject attrs:
                    this.attrs = attrs;
                    break;
            }
        }

        isValid &= Type != MarkType.Undefined;

        return isValid;
    }

    public int GetIntAttr(string name, int defaultValue = 0)
    {
        return attrs.GetIntAttr(name, defaultValue);
    }

    public string GetStringAttr(string name, string defaultValue = "")
    {
        return attrs.GetStringAttr(name, defaultValue);
    }
}
