// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Jint.Runtime.Descriptors;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentFieldProperty : PropertyDescriptor
    {
        private readonly ContentFieldObject contentField;
        private IJsonValue contentValue;
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

        public IJsonValue ContentValue
        {
            get { return contentValue ?? (contentValue = JsonMapper.Map(value)); }
        }

        public bool IsChanged
        {
            get { return isChanged; }
        }

        public ContentFieldProperty(ContentFieldObject contentField, IJsonValue contentValue = null)
            : base(null, true, true, true)
        {
            this.contentField = contentField;
            this.contentValue = contentValue;
        }
    }
}