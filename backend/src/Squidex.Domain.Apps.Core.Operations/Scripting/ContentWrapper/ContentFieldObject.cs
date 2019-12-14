﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

#pragma warning disable RECS0133 // Parameter name differs in base declaration

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentFieldObject : ObjectInstance
    {
        private readonly ContentDataObject contentData;
        private readonly ContentFieldData? fieldData;
        private HashSet<string> valuesToDelete;
        private Dictionary<string, PropertyDescriptor> valueProperties;
        private bool isChanged;

        public ContentFieldData? FieldData
        {
            get { return fieldData; }
        }

        public ContentFieldObject(ContentDataObject contentData, ContentFieldData? fieldData, bool isNew)
            : base(contentData.Engine)
        {
            Extensible = true;

            this.contentData = contentData;
            this.fieldData = fieldData;

            if (isNew)
            {
                MarkChanged();
            }
        }

        public void MarkChanged()
        {
            isChanged = true;

            contentData.MarkChanged();
        }

        public bool TryUpdate(out ContentFieldData? result)
        {
            result = fieldData;

            if (isChanged && fieldData != null)
            {
                if (valuesToDelete != null)
                {
                    foreach (var field in valuesToDelete)
                    {
                        fieldData.Remove(field);
                    }
                }

                if (valueProperties != null)
                {
                    foreach (var (key, propertyDescriptor) in valueProperties)
                    {
                        var value = (ContentFieldProperty)propertyDescriptor;

                        if (value.IsChanged)
                        {
                            fieldData[key] = value.ContentValue;
                        }
                    }
                }
            }

            return isChanged;
        }

        public override void RemoveOwnProperty(string propertyName)
        {
            if (valuesToDelete == null)
            {
                valuesToDelete = new HashSet<string>();
            }

            valuesToDelete.Add(propertyName);
            valueProperties?.Remove(propertyName);

            MarkChanged();
        }

        public override bool DefineOwnProperty(string propertyName, PropertyDescriptor desc, bool throwOnError)
        {
            EnsurePropertiesInitialized();

            if (!valueProperties.ContainsKey(propertyName))
            {
                valueProperties[propertyName] = new ContentFieldProperty(this) { Value = desc.Value };
            }

            return true;
        }

        public override PropertyDescriptor GetOwnProperty(string propertyName)
        {
            EnsurePropertiesInitialized();

            return valueProperties?.GetOrDefault(propertyName) ?? PropertyDescriptor.Undefined;
        }

        public override IEnumerable<KeyValuePair<string, PropertyDescriptor>> GetOwnProperties()
        {
            EnsurePropertiesInitialized();

            return valueProperties;
        }

        private void EnsurePropertiesInitialized()
        {
            if (valueProperties == null)
            {
                valueProperties = new Dictionary<string, PropertyDescriptor>(fieldData?.Count ?? 0);

                if (fieldData != null)
                {
                    foreach (var (key, value) in fieldData)
                    {
                        valueProperties.Add(key, new ContentFieldProperty(this, value));
                    }
                }
            }
        }
    }
}
