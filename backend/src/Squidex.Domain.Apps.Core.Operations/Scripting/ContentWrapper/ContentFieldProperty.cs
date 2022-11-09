// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Jint.Native;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

public sealed class ContentFieldProperty : CustomProperty
{
    private readonly ContentFieldObject contentField;
    private JsonValue contentValue;
    private JsValue? value;
    private bool isChanged;

    [DebuggerHidden]
    protected override JsValue? CustomValue
    {
        get
        {
            if (value == null)
            {
                if (contentValue != default)
                {
                    value = JsonMapper.Map(contentValue, contentField.Engine);
                }
            }

            return value;
        }
        set
        {
            var newContentValue = JsonMapper.Map(value);

            if (!Equals(contentValue, newContentValue))
            {
                this.value = value;

                contentValue = newContentValue;
                contentField.MarkChanged();

                isChanged = true;
            }
        }
    }

    public JsonValue ContentValue
    {
        get => contentValue;
    }

    public bool IsChanged
    {
        get => isChanged;
    }

    public ContentFieldProperty(ContentFieldObject contentField, JsonValue contentValue = default)
    {
        this.contentField = contentField;
        this.contentValue = contentValue;
    }
}
