// ==========================================================================
//  ContentFieldProperty.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Jint.Native;
using Jint.Runtime.Descriptors;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentFieldProperty : PropertyDescriptor
    {
        private readonly ContentFieldObject contentField;
        private JToken contentValue;
        private JsValue value;
        private bool isChanged;

        public override JsValue Value
        {
            get
            {
                return value ?? (value = JsonMapper.Map(contentValue, contentField.Engine));
            }
            set
            {
                if (!Equals(this.value, value))
                {
                    this.value = value;

                    contentValue = null;
                    contentField.MarkChanged();

                    isChanged = true;
                }
            }
        }

        public JToken ContentValue
        {
            get { return contentValue ?? (contentValue = JsonMapper.Map(value)); }
        }

        public bool IsChanged
        {
            get { return isChanged; }
        }

        public ContentFieldProperty(ContentFieldObject contentField, JToken contentValue = null)
            : base(null, true, true, true)
        {
            this.contentField = contentField;
            this.contentValue = contentValue;
        }
    }
}