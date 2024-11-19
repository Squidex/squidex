// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Jint.Native;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

public sealed class ContentFieldProperty(ContentFieldObject contentField, JsonValue contentValue = default) : CustomProperty
{
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
}
