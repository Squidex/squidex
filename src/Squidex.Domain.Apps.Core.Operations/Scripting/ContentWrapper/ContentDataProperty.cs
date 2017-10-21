// ==========================================================================
//  ContentFieldProperty.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentDataProperty : PropertyDescriptor
    {
        private readonly ContentDataObject contentData;
        private ContentFieldObject contentField;
        private JsValue value;

        public override JsValue Value
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
                        throw new JavaScriptException("Can only assign object to content data.");
                    }

                    var obj = value.AsObject();

                    contentField = new ContentFieldObject(contentData, new ContentFieldData(), true);

                    foreach (var kvp in obj.GetOwnProperties())
                    {
                        contentField.Put(kvp.Key, kvp.Value.Value, true);
                    }

                    this.value = new JsValue(contentField);
                }
            }
        }

        public ContentFieldObject ContentField
        {
            get { return contentField; }
        }

        public ContentDataProperty(ContentDataObject contentData, ContentFieldObject contentField = null)
            : base(null, true, true, true)
        {
            this.contentData = contentData;
            this.contentField = contentField;

            if (contentField != null)
            {
                value = new JsValue(contentField);
            }
        }
    }
}