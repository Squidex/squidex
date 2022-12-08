// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Text;

namespace Squidex.Infrastructure.Queries.OData;

public static class EdmModelConverter
{
    private const int MaxDepth = 7;

    public static EdmModel ConvertToEdm(this QueryModel queryModel, string modelName, string name)
    {
        var model = new EdmModel();

        var entityType = new EdmEntityType(modelName, name);
        var entityPath = new Stack<string>();

        void Convert(EdmStructuredType target, FilterSchema schema)
        {
            if (schema.Fields == null)
            {
                return;
            }

            foreach (var field in FilterSchema.GetConflictFreeFields(schema.Fields))
            {
                var fieldName = field.Path.EscapeEdmField();

                switch (field.Schema.Type)
                {
                    case FilterSchemaType.Boolean:
                        target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.Boolean, field.IsNullable);
                        break;
                    case FilterSchemaType.DateTime:
                        target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.DateTimeOffset, field.IsNullable);
                        break;
                    case FilterSchemaType.GeoObject:
                        target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.GeographyPoint, field.IsNullable);
                        break;
                    case FilterSchemaType.Guid:
                        target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.Guid, field.IsNullable);
                        break;
                    case FilterSchemaType.Number:
                        target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.Double, field.IsNullable);
                        break;
                    case FilterSchemaType.String:
                    case FilterSchemaType.StringArray:
                        target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.String, field.IsNullable);
                        break;
                    case FilterSchemaType.Object:
                    case FilterSchemaType.ObjectArray:
                        {
                            if (field.Schema.Fields == null || field.Schema.Fields.Count == 0 || entityPath.Count >= MaxDepth)
                            {
                                break;
                            }

                            entityPath.Push(fieldName);

                            var typeName = string.Join("_", entityPath.Reverse().Select(x => x.EscapeEdmField().ToPascalCase()));

                            var result = model.SchemaElements.OfType<EdmComplexType>().FirstOrDefault(x => x.Name == typeName);

                            if (result == null)
                            {
                                result = new EdmComplexType(modelName, typeName);

                                model.AddElement(result);

                                Convert(result, field.Schema);
                            }

                            target.AddStructuralProperty(fieldName, new EdmComplexTypeReference(result, field.IsNullable));

                            entityPath.Pop();
                            break;
                        }

                    case FilterSchemaType.Any:
                        {
                            var result = model.SchemaElements.OfType<EdmComplexType>().FirstOrDefault(x => x.Name == "Any");

                            if (result == null)
                            {
                                result = new EdmComplexType("Squidex", "Any", null, false, true);

                                model.AddElement(result);
                            }

                            target.AddStructuralProperty(fieldName, new EdmComplexTypeReference(result, field.IsNullable));
                            break;
                        }
                }
            }
        }

        Convert(entityType, queryModel.Schema);

        var container = new EdmEntityContainer("Squidex", "Container");

        container.AddEntitySet("ContentSet", entityType);

        model.AddElement(container);
        model.AddElement(entityType);

        return model;
    }

    public static string EscapeEdmField(this string field)
    {
        return field.Replace("-", "_", StringComparison.Ordinal);
    }

    public static string UnescapeEdmField(this string field)
    {
        return field.Replace("_", "-", StringComparison.Ordinal);
    }
}
