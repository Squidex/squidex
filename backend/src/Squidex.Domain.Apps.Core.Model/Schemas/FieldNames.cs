// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed class FieldNames : ReadonlyList<string>
{
    private const string MetaPrefix = "meta.";
    private const string DataPrefix = "data.";
    private static readonly HashSet<string> MetaFields =
    [
        "id",
        "created",
        "createdBy.avatar",
        "createdBy.name",
        "lastModified",
        "lastModifiedBy.avatar",
        "lastModifiedBy.name",
        "status",
        "status.color",
        "status.next",
        "version",
        "translationStatus",
        "translationStatusAverage"
    ];

    public static readonly new FieldNames Empty = new FieldNames(new List<string>());

    public FieldNames()
    {
    }

    public FieldNames(IList<string> list)
        : base(list)
    {
    }

    public static FieldNames Create(params string[] names)
    {
        return new FieldNames(names.ToList());
    }

    public FieldNames Add(string field)
    {
        var list = this.ToList();

        list.Add(field);

        return new FieldNames(list);
    }

    public FieldNames Remove(string field)
    {
        var list = this.ToList();

        list.Remove(field);

        return new FieldNames(list);
    }

    public static bool IsMetaField(string? name)
    {
        return name != null && MetaFields.Contains(name);
    }

    public static bool IsDataField(string? name)
    {
        return name?.StartsWith(DataPrefix, StringComparison.OrdinalIgnoreCase) == true;
    }

    public static bool IsDataField(string? name, [MaybeNullWhen(false)] out string dataField)
    {
        dataField = null!;

        if (IsDataField(name))
        {
            dataField = name![MetaPrefix.Length..];
            return true;
        }

        return false;
    }

    public FieldNames Migrate()
    {
        static bool IsOldMetaField(string name)
        {
            return name.StartsWith(MetaPrefix, StringComparison.OrdinalIgnoreCase);
        }

        var isNewVersion = this.All(x => IsDataField(x) || IsMetaField(x));

        if (isNewVersion)
        {
            return this;
        }

        var result = this.ToList();

        for (var i = 0; i < result.Count; i++)
        {
            var field = result[i];

            if (IsOldMetaField(field))
            {
                field = field[MetaPrefix.Length..];
            }
            else
            {
                field = $"{DataPrefix}{field}";
            }

            result[i] = field;
        }

        return new FieldNames(result);
    }
}
