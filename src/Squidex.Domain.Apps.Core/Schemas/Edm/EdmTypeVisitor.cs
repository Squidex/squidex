// ==========================================================================
//  EdmTypeVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.OData.Edm;

namespace Squidex.Domain.Apps.Core.Schemas.Edm
{
    public sealed class EdmTypeVisitor : IFieldVisitor<IEdmTypeReference>
    {
        private static readonly EdmTypeVisitor Instance = new EdmTypeVisitor();

        private EdmTypeVisitor()
        {
        }

        public static IEdmTypeReference CreateEdmType(Field field)
        {
            return field.Accept(Instance);
        }

        public IEdmTypeReference Visit(AssetsField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference Visit(BooleanField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Boolean, field);
        }

        public IEdmTypeReference Visit(DateTimeField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.DateTimeOffset, field);
        }

        public IEdmTypeReference Visit(GeolocationField field)
        {
            return null;
        }

        public IEdmTypeReference Visit(JsonField field)
        {
            return null;
        }

        public IEdmTypeReference Visit(NumberField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.Double, field);
        }

        public IEdmTypeReference Visit(ReferencesField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference Visit(StringField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        public IEdmTypeReference Visit(TagsField field)
        {
            return CreatePrimitive(EdmPrimitiveTypeKind.String, field);
        }

        private static IEdmTypeReference CreatePrimitive(EdmPrimitiveTypeKind kind, Field field)
        {
            return EdmCoreModel.Instance.GetPrimitive(kind, !field.RawProperties.IsRequired);
        }
    }
}
