// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Text;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class EdmModelConverter
    {
        public static EdmModel ConvertToEdm(this QueryModel queryModel, string modelName, string name)
        {
            var model = new EdmModel();

            var entityType = new EdmEntityType(modelName, name);
            var entityPath = new Stack<string>();

            void Convert(EdmStructuredType target, IReadOnlyList<FilterableField> fields)
            {
                foreach (var field in queryModel.Fields)
                {
                    var fieldName = field.Path.EscapeEdmField();

                    switch (field.Type)
                    {
                        case FilterableFieldType.Boolean:
                            target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.Boolean, field.IsNullable);
                            break;
                        case FilterableFieldType.DateTime:
                            target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.DateTimeOffset, field.IsNullable);
                            break;
                        case FilterableFieldType.GeoObject:
                            target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.GeographyPoint, field.IsNullable);
                            break;
                        case FilterableFieldType.Guid:
                            target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.Guid, field.IsNullable);
                            break;
                        case FilterableFieldType.Number:
                            target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.Double, field.IsNullable);
                            break;
                        case FilterableFieldType.String:
                        case FilterableFieldType.StringArray:
                            target.AddStructuralProperty(fieldName, EdmPrimitiveTypeKind.String, field.IsNullable);
                            break;
                        case FilterableFieldType.Object:
                        case FilterableFieldType.ObjectArray:
                            {
                                if (field.Fields == null || field.Fields.Count == 0)
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

                                    Convert(result, field.Fields);
                                }

                                target.AddStructuralProperty("data", new EdmComplexTypeReference(result, field.IsNullable));

                                entityPath.Pop();
                                break;
                            }

                        case FilterableFieldType.Any:
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

            Convert(entityType, queryModel.Fields);

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
}
