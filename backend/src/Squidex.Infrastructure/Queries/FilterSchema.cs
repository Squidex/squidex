// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries;

public sealed record FilterSchema(FilterSchemaType Type)
{
    public static readonly FilterSchema Any = new FilterSchema(FilterSchemaType.Any);
    public static readonly FilterSchema Boolean = new FilterSchema(FilterSchemaType.Boolean);
    public static readonly FilterSchema Date = new FilterSchema(FilterSchemaType.Date);
    public static readonly FilterSchema DateTime = new FilterSchema(FilterSchemaType.DateTime);
    public static readonly FilterSchema GeoObject = new FilterSchema(FilterSchemaType.GeoObject);
    public static readonly FilterSchema Guid = new FilterSchema(FilterSchemaType.Guid);
    public static readonly FilterSchema Number = new FilterSchema(FilterSchemaType.Number);
    public static readonly FilterSchema String = new FilterSchema(FilterSchemaType.String);
    public static readonly FilterSchema StringArray = new FilterSchema(FilterSchemaType.StringArray);

    public ReadonlyList<FilterField>? Fields { get; init; }

    public object? Extra { get; init; }

    public FilterSchema Flatten(int maxDepth = 7, Predicate<FilterSchema>? predicate = null)
    {
        if (Fields == null || Fields.Count == 0)
        {
            return this;
        }

        var result = new List<FilterField>();

        var pathStack = new Stack<string>();

        void AddField(FilterField field)
        {
            pathStack.Push(field.Path);

            if (predicate?.Invoke(field.Schema) != false)
            {
                var path = string.Join('.', pathStack.Reverse());

                var schema = field.Schema with
                {
                    Fields = null
                };

                result?.Add(field with { Path = path, Schema = schema });
            }

            if (field.Schema.Fields?.Count > 0 && pathStack.Count < maxDepth)
            {
                AddFields(field.Schema.Fields);
            }

            pathStack.Pop();
        }

        void AddFields(IEnumerable<FilterField> source)
        {
            foreach (var field in source)
            {
                AddField(field);
            }
        }

        AddFields(Fields);

        var conflictFree = GetConflictFreeFields(result);

        return this with
        {
            Fields = conflictFree.ToReadonlyList()
        };
    }

    public static IEnumerable<FilterField> GetConflictFreeFields(IEnumerable<FilterField> fields)
    {
        var conflictFree = fields.GroupBy(x => x.Path).Select(group =>
        {
            var firstType = group.First().Schema.Type;

            if (group.Count() == 1)
            {
                return group.Take(1);
            }
            else if (group.All(x => x.Schema.Type == firstType))
            {
                return group.Take(1).Select(x => x with { Description = null });
            }
            else
            {
                return Enumerable.Empty<FilterField>();
            }
        }).SelectMany(x => x);

        return conflictFree;
    }
}
