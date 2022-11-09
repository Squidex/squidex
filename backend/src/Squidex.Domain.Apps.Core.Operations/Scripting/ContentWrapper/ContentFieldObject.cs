// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

public sealed class ContentFieldObject : ObjectInstance
{
    private readonly ContentDataObject contentData;
    private readonly ContentFieldData? fieldData;
    private HashSet<string>? valuesToDelete;
    private Dictionary<string, PropertyDescriptor> valueProperties;
    private bool isChanged;

    public ContentFieldData? FieldData
    {
        get => fieldData;
    }

    public override bool Extensible => true;

    public ContentFieldObject(ContentDataObject contentData, ContentFieldData? fieldData, bool isNew)
        : base(contentData.Engine)
    {
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

    public override void RemoveOwnProperty(JsValue property)
    {
        var propertyName = property.AsString();

        valuesToDelete ??= new HashSet<string>();
        valuesToDelete.Add(propertyName);

        valueProperties?.Remove(propertyName);

        MarkChanged();
    }

    public override bool Set(JsValue property, JsValue value, JsValue receiver)
    {
        EnsurePropertiesInitialized();

        var propertyName = property.AsString();

        valueProperties.GetOrAdd(propertyName, _ => new ContentFieldProperty(this)).Value = value;

        return true;
    }

    public override bool DefineOwnProperty(JsValue property, PropertyDescriptor desc)
    {
        EnsurePropertiesInitialized();

        var propertyName = property.AsString();

        if (!valueProperties.ContainsKey(propertyName))
        {
            valueProperties[propertyName] = new ContentFieldProperty(this) { Value = desc.Value };
        }

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

        return valueProperties?.GetValueOrDefault(propertyName) ?? PropertyDescriptor.Undefined;
    }

    public override IEnumerable<KeyValuePair<JsValue, PropertyDescriptor>> GetOwnProperties()
    {
        EnsurePropertiesInitialized();

        return valueProperties.Select(x => new KeyValuePair<JsValue, PropertyDescriptor>(x.Key, x.Value));
    }

    public override List<JsValue> GetOwnPropertyKeys(Types types = Types.String | Types.Symbol)
    {
        EnsurePropertiesInitialized();

        return valueProperties.Keys.Select(x => (JsValue)x).ToList();
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

    public override object ToObject()
    {
        if (TryUpdate(out var result))
        {
            return result!;
        }

        return fieldData!;
    }
}
