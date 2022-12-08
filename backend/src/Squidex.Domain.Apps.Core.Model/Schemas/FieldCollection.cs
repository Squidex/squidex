// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed class FieldCollection<T> where T : IField
{
    public static readonly FieldCollection<T> Empty = new FieldCollection<T>();

    private static readonly Dictionary<long, T> EmptyById = new Dictionary<long, T>();
    private static readonly Dictionary<string, T> EmptyByString = new Dictionary<string, T>();

    private readonly T[] fieldsOrdered;
    private Dictionary<long, T>? fieldsById;
    private Dictionary<string, T>? fieldsByName;

    public IReadOnlyList<T> Ordered
    {
        get => fieldsOrdered;
    }

    public IReadOnlyDictionary<long, T> ById
    {
        get
        {
            if (fieldsById == null)
            {
                if (fieldsOrdered.Length == 0)
                {
                    fieldsById = EmptyById;
                }
                else
                {
                    fieldsById = fieldsOrdered.ToDictionary(x => x.Id);
                }
            }

            return fieldsById;
        }
    }

    public IReadOnlyDictionary<string, T> ByName
    {
        get
        {
            if (fieldsByName == null)
            {
                if (fieldsOrdered.Length == 0)
                {
                    fieldsByName = EmptyByString;
                }
                else
                {
                    fieldsByName = fieldsOrdered.ToDictionary(x => x.Name);
                }
            }

            return fieldsByName;
        }
    }

    private FieldCollection()
    {
        fieldsOrdered = Array.Empty<T>();
    }

    public FieldCollection(T[] fields)
    {
        Guard.NotNull(fields);

        fieldsOrdered = fields;
    }

    private FieldCollection(IEnumerable<T> fields)
    {
        fieldsOrdered = fields.ToArray();
    }

    [Pure]
    public FieldCollection<T> Remove(long fieldId)
    {
        if (!ById.TryGetValue(fieldId, out _))
        {
            return this;
        }

        return new FieldCollection<T>(fieldsOrdered.Where(x => x.Id != fieldId));
    }

    [Pure]
    public FieldCollection<T> Reorder(List<long> ids)
    {
        Guard.NotNull(ids);

        if (ids.Count != fieldsOrdered.Length || ids.Any(x => !ById.ContainsKey(x)))
        {
            ThrowHelper.ArgumentException("Ids must cover all fields.", nameof(ids));
        }

        if (ids.SequenceEqual(fieldsOrdered.Select(x => x.Id)))
        {
            return this;
        }

        return new FieldCollection<T>(fieldsOrdered.OrderBy(f => ids.IndexOf(f.Id)));
    }

    [Pure]
    public FieldCollection<T> Add(T field)
    {
        Guard.NotNull(field);

        if (ByName.ContainsKey(field.Name))
        {
            ThrowHelper.ArgumentException($"A field with name '{field.Name}' already exists.", nameof(field));
        }

        if (ById.ContainsKey(field.Id))
        {
            ThrowHelper.ArgumentException($"A field with ID {field.Id} already exists.", nameof(field));
        }

        return new FieldCollection<T>(fieldsOrdered.Union(Enumerable.Repeat(field, 1)));
    }

    [Pure]
    public FieldCollection<T> Update(long fieldId, Func<T, T> updater)
    {
        Guard.NotNull(updater);

        if (!ById.TryGetValue(fieldId, out var field))
        {
            return this;
        }

        var newField = updater(field);

        if (ReferenceEquals(newField, field))
        {
            return this;
        }

        if (newField is null)
        {
            ThrowHelper.InvalidOperationException($"Field must be of type {typeof(T)}");
            return default!;
        }

        return new FieldCollection<T>(fieldsOrdered.Select(x => ReferenceEquals(x, field) ? newField : x));
    }
}
