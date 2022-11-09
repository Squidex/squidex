// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents;

public sealed class ContentData : Dictionary<string, ContentFieldData?>, IEquatable<ContentData>
{
    public ContentData()
        : base(StringComparer.Ordinal)
    {
    }

    public ContentData(ContentData source)
        : base(source, StringComparer.Ordinal)
    {
    }

    public ContentData(int capacity)
        : base(capacity, StringComparer.Ordinal)
    {
    }

    public ContentData AddField(string name, ContentFieldData? data)
    {
        Guard.NotNullOrEmpty(name);

        this[name] = data;

        return this;
    }

    public ContentData UseSameFields(ContentData? other)
    {
        if (other == null || other.Count == 0)
        {
            return this;
        }

        foreach (var (fieldName, fieldData) in this.ToList())
        {
            if (fieldData == null)
            {
                continue;
            }

            if (!other.TryGetValue(fieldName, out var otherField) || otherField == null)
            {
                continue;
            }

            if (otherField.Equals(fieldData))
            {
                this[fieldName] = otherField;
            }
            else
            {
                foreach (var (language, value) in fieldData.ToList())
                {
                    if (!otherField.TryGetValue(language, out var otherValue))
                    {
                        continue;
                    }

                    if (otherValue.Equals(value))
                    {
                        fieldData[language] = otherValue;
                    }
                }
            }
        }

        return this;
    }

    private static ContentData MergeTo(ContentData target, params ContentData[] sources)
    {
        Guard.NotEmpty(sources);

        if (sources.Length == 1 || sources.Skip(1).All(x => ReferenceEquals(x, sources[0])))
        {
            return sources[0];
        }

        foreach (var source in sources)
        {
            foreach (var (fieldName, sourceFieldData) in source)
            {
                if (sourceFieldData == null)
                {
                    continue;
                }

                var targetFieldData = target.GetOrAdd(fieldName, _ => new ContentFieldData());

                if (targetFieldData == null)
                {
                    continue;
                }

                foreach (var (partition, value) in sourceFieldData)
                {
                    targetFieldData[partition] = value;
                }
            }
        }

        return target;
    }

    public static ContentData Merge(params ContentData[] contents)
    {
        return MergeTo(new ContentData(), contents);
    }

    public ContentData MergeInto(ContentData target)
    {
        return Merge(target, this);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ContentData);
    }

    public bool Equals(ContentData? other)
    {
        return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
    }

    public override int GetHashCode()
    {
        return this.DictionaryHashCode();
    }

    public override string ToString()
    {
        return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value}"))}}}";
    }

    public ContentData Clone()
    {
        var clone = new ContentData(Count);

        foreach (var (key, value) in this)
        {
            clone[key] = value?.Clone()!;
        }

        return clone;
    }
}
