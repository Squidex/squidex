// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public sealed class ContentDataObject : ObjectInstance
    {
        private readonly ContentData contentData;
        private HashSet<string> fieldsToDelete;
        private Dictionary<string, PropertyDescriptor> fieldProperties;
        private bool isChanged;

        public override bool Extensible => true;

        public ContentDataObject(Engine engine, ContentData contentData)
            : base(engine)
        {
            this.contentData = contentData;
        }

        public void MarkChanged()
        {
            isChanged = true;
        }

        public bool TryUpdate(out ContentData result)
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
                    foreach (var (key, propertyDescriptor) in fieldProperties)
                    {
                        var value = (ContentDataProperty)propertyDescriptor;

                        if (value.ContentField != null && value.ContentField.TryUpdate(out var fieldData))
                        {
                            contentData[key] = fieldData;
                        }
                    }
                }
            }

            return isChanged;
        }

        public override void RemoveOwnProperty(JsValue property)
        {
            var propertyName = property.AsString();

            fieldsToDelete ??= new HashSet<string>();
            fieldsToDelete.Add(propertyName);

            fieldProperties?.Remove(propertyName);

            MarkChanged();
        }

        public override bool DefineOwnProperty(JsValue property, PropertyDescriptor desc)
        {
            EnsurePropertiesInitialized();

            var propertyName = property.AsString();

            if (!fieldProperties.ContainsKey(propertyName))
            {
                fieldProperties[propertyName] = new ContentDataProperty(this) { Value = desc.Value };
            }

            return true;
        }

        public override bool Set(JsValue property, JsValue value, JsValue receiver)
        {
            EnsurePropertiesInitialized();

            var propertyName = property.AsString();

            fieldProperties.GetOrAdd(propertyName, this, (k, c) => new ContentDataProperty(c)).Value = value;

            return true;
        }

        public override PropertyDescriptor GetOwnProperty(JsValue property)
        {
            EnsurePropertiesInitialized();

            var propertyName = property.AsString();

            if (propertyName.Equals("toJSON", StringComparison.OrdinalIgnoreCase))
            {
                return PropertyDescriptor.Undefined;
            }

            return fieldProperties.GetOrAdd(propertyName, this, (k, c) => new ContentDataProperty(c, new ContentFieldObject(c, new ContentFieldData(), false)));
        }

        public override IEnumerable<KeyValuePair<JsValue, PropertyDescriptor>> GetOwnProperties()
        {
            EnsurePropertiesInitialized();

            return fieldProperties.Select(x => new KeyValuePair<JsValue, PropertyDescriptor>(x.Key, x.Value));
        }

        public override List<JsValue> GetOwnPropertyKeys(Types types = Types.String | Types.Symbol)
        {
            EnsurePropertiesInitialized();

            return fieldProperties.Keys.Select(x => (JsValue)x).ToList();
        }

        private void EnsurePropertiesInitialized()
        {
            if (fieldProperties == null)
            {
                fieldProperties = new Dictionary<string, PropertyDescriptor>(contentData.Count);

                foreach (var (key, value) in contentData)
                {
                    fieldProperties.Add(key, new ContentDataProperty(this, new ContentFieldObject(this, value, false)));
                }
            }
        }
    }
}
