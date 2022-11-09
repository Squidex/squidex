// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

public sealed class ContentDataProperty : CustomProperty
{
    private readonly ContentDataObject contentData;
    private ContentFieldObject? contentField;
    private JsValue value;

    protected override JsValue CustomValue
    {
        get
        {
            return value;
        }
        set
        {
            if (!Equals(this.value, value))
            {
                if (value == null || !value.IsObject())
                {
                    throw new JavaScriptException("You can only assign objects to content data.");
                }

                var obj = value.AsObject();

                contentField = new ContentFieldObject(contentData, new ContentFieldData(), true);

                foreach (var (key, propertyDescriptor) in obj.GetOwnProperties())
                {
                    contentField.Set(key, propertyDescriptor.Value);
                }

                this.value = contentField;
            }
        }
    }

    public ContentFieldObject? ContentField
    {
        get => contentField;
    }

    public ContentDataProperty(ContentDataObject contentData, ContentFieldObject? contentField = null)
    {
        this.contentData = contentData;
        this.contentField = contentField;

        if (contentField != null)
        {
            value = contentField;
        }
    }
}
