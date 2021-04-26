// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentFieldProperty : CustomProperty
    {
        private readonly ContentFieldObject contentField;
        private IJsonValue? contentValue;
        private JsValue? value;
        private bool isChanged;

        protected override JsValue? CustomValue
        {
            get
            {
                if (value == null)
                {
                    if (contentValue != null)
                    {
                        value = JsonMapper.Map(contentValue, contentField.Engine);
                    }
                }

                return value;
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
            get => contentValue ??= JsonMapper.Map(value);
        }

        public bool IsChanged
        {
            get => isChanged;
        }

        public ContentFieldProperty(ContentFieldObject contentField, IJsonValue? contentValue = null)
        {
            this.contentField = contentField;
            this.contentValue = contentValue;
        }
    }
}