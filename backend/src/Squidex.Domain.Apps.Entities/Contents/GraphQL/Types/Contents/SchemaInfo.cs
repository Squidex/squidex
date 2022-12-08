// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Text;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class SchemaInfo
{
    public ISchemaEntity Schema { get; }

    public string DisplayName => Schema.DisplayName();

    public string ComponentType { get; }

    public string ContentType { get; }

    public string DataFlatType { get; }

    public string DataInputType { get; }

    public string DataType { get; }

    public string ContentResultType { get; }

    public string TypeName { get; }

    public IReadOnlyList<FieldInfo> Fields { get; init; }

    private SchemaInfo(ISchemaEntity schema, string typeName, ReservedNames typeNames)
    {
        Schema = schema;

        ComponentType = typeNames[$"{typeName}Component"];
        ContentResultType = typeNames[$"{typeName}ResultDto"];
        ContentType = typeName;
        DataFlatType = typeNames[$"{typeName}FlatDataDto"];
        DataInputType = typeNames[$"{typeName}DataInputDto"];
        DataType = typeNames[$"{typeName}DataDto"];
        TypeName = typeName;
    }

    public override string ToString()
    {
        return TypeName;
    }

    public static IEnumerable<SchemaInfo> Build(IEnumerable<ISchemaEntity> schemas, ReservedNames typeNames)
    {
        foreach (var schema in schemas.OrderBy(x => x.Created))
        {
            var typeName = typeNames[schema.TypeName()];

            yield return new SchemaInfo(schema, typeName, typeNames)
            {
                Fields = FieldInfo.Build(schema.SchemaDef.Fields, $"{typeName}Data", typeNames).ToList()
            };
        }
    }
}

internal sealed class FieldInfo
{
    public IField Field { get; set; }

    public string DisplayName => Field.DisplayName();

    public string EmbeddableStringType { get; }

    public string EmbeddedEnumType { get; }

    public string FieldName { get; }

    public string FieldNameDynamic { get; }

    public string LocalizedInputType { get; }

    public string LocalizedType { get; }

    public string LocalizedTypeDynamic { get; }

    public string NestedInputType { get; }

    public string NestedType { get; }

    public string UnionComponentType { get; }

    public string UnionReferenceType { get; }

    public IReadOnlyList<FieldInfo> Fields { get; init; }

    private FieldInfo(IField field, string fieldName, string typeName, ReservedNames typeNames)
    {
        Field = field;

        EmbeddableStringType = typeNames[$"{typeName}EmbeddableString"];
        EmbeddedEnumType = typeNames[$"{typeName}Enum"];
        FieldName = fieldName;
        FieldNameDynamic = $"{fieldName}__Dynamic";
        LocalizedInputType = typeNames[$"{typeName}InputDto"];
        LocalizedType = typeNames[$"{typeName}Dto"];
        LocalizedTypeDynamic = typeNames[$"{typeName}Dto__Dynamic"];
        NestedInputType = typeNames[$"{typeName}ChildInputDto"];
        NestedType = typeNames[$"{typeName}ChildDto"];
        UnionComponentType = typeNames[$"{typeName}ComponentUnionDto"];
        UnionReferenceType = typeNames[$"{typeName}UnionDto"];
    }

    public override string ToString()
    {
        return FieldName;
    }

    internal static IEnumerable<FieldInfo> Build(IEnumerable<IField> fields, string typeName, ReservedNames typeNames)
    {
        var typeScope = ReservedNames.ForFields();

        foreach (var field in fields.ForApi())
        {
            // Field names must be unique within the scope of the parent type.
            var fieldName = typeScope[field.Name.ToCamelCase()];

            // Type names must be globally unique.
            var fieldTypeName = typeNames[$"{typeName}{field.TypeName()}"];

            var nested = new List<FieldInfo>();

            if (field is IArrayField arrayField)
            {
                nested = Build(arrayField.Fields, fieldTypeName, typeNames).ToList();
            }

            yield return new FieldInfo(
                field,
                fieldName,
                fieldTypeName,
                typeNames)
            {
                Fields = nested
            };
        }
    }
}
