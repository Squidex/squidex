// ==========================================================================
//  ContentDataObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

#pragma warning disable RECS0133 // Parameter name differs in base declaration

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentDataObject : ObjectInstance
    {
        private readonly NamedContentData contentData;
        private HashSet<string> fieldsToDelete;
        private Dictionary<string, ContentDataProperty> fieldProperties;
        private bool isChanged;

        public ContentDataObject(Engine engine, NamedContentData contentData)
            : base(engine)
        {
            Extensible = true;

            this.contentData = contentData;
        }

        public void MarkChanged()
        {
            isChanged = true;
        }

        public bool TryUpdate(out NamedContentData result)
        {
            result = contentData;

            if (isChanged)
            {
                if (fieldsToDelete != null)
                {
                    foreach (var field in fieldsToDelete)
                    {
                        contentData.Remove(field);
                    }
                }

                if (fieldProperties != null)
                {
                    foreach (var kvp in fieldProperties)
                    {
                        if (kvp.Value.ContentField.TryUpdate(out var fieldData))
                        {
                            contentData[kvp.Key] = fieldData;
                        }
                    }
                }
            }

            return isChanged;
        }

        public override void RemoveOwnProperty(string propertyName)
        {
            if (fieldsToDelete == null)
            {
                fieldsToDelete = new HashSet<string>();
            }

            fieldsToDelete.Add(propertyName);
            fieldProperties?.Remove(propertyName);

            MarkChanged();
        }

        public override bool DefineOwnProperty(string propertyName, PropertyDescriptor desc, bool throwOnError)
        {
            EnsurePropertiesInitialized();

            if (!fieldProperties.ContainsKey(propertyName))
            {
                fieldProperties[propertyName] = new ContentDataProperty(this) { Value = desc.Value };
            }

            return true;
        }

        public override void Put(string propertyName, JsValue value, bool throwOnError)
        {
            EnsurePropertiesInitialized();

            fieldProperties.GetOrAdd(propertyName, x => new ContentDataProperty(this)).Value = value;
        }

        public override PropertyDescriptor GetOwnProperty(string propertyName)
        {
            EnsurePropertiesInitialized();

            return fieldProperties.GetOrDefault(propertyName) ?? new PropertyDescriptor(new ObjectInstance(Engine) { Extensible = true }, true, false, true);
        }

        public override IEnumerable<KeyValuePair<string, PropertyDescriptor>> GetOwnProperties()
        {
            EnsurePropertiesInitialized();

            foreach (var property in fieldProperties)
            {
                yield return new KeyValuePair<string, PropertyDescriptor>(property.Key, property.Value);
            }
        }

        private void EnsurePropertiesInitialized()
        {
            if (fieldProperties == null)
            {
                fieldProperties = new Dictionary<string, ContentDataProperty>(contentData.Count);

                foreach (var kvp in contentData)
                {
                    fieldProperties.Add(kvp.Key, new ContentDataProperty(this, new ContentFieldObject(this, kvp.Value, false)));
                }
            }
        }
    }
}
