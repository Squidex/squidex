// ==========================================================================
//  ContentFieldObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentFieldObject : ObjectInstance
    {
        private readonly ContentDataObject contentData;
        private readonly ContentFieldData fieldData;
        private HashSet<string> valuesToDelete;
        private Dictionary<string, ContentFieldProperty> valueProperties;
        private bool isChanged;

        public bool IsChanged
        {
            get { return isChanged; }
        }

        public ContentFieldData FieldData
        {
            get { return fieldData; }
        }

        public ContentFieldObject(ContentDataObject contentData, ContentFieldData fieldData, bool isNew)
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

        public bool TryUpdate(out ContentFieldData result)
        {
            result = fieldData;

            if (isChanged)
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
                    foreach (var kvp in valueProperties)
                    {
                        if (kvp.Value.IsChanged)
                        {
                            fieldData[kvp.Key] = kvp.Value.ContentValue;
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

            foreach (var property in valueProperties)
            {
                yield return new KeyValuePair<string, PropertyDescriptor>(property.Key, property.Value);
            }
        }

        private void EnsurePropertiesInitialized()
        {
            if (valueProperties == null)
            {
                valueProperties = new Dictionary<string, ContentFieldProperty>(FieldData.Count);

                foreach (var kvp in FieldData)
                {
                    valueProperties.Add(kvp.Key, new ContentFieldProperty(this, kvp.Value));
                }
            }
        }
    }
}
