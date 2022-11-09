// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal sealed class FieldInputVisitor : IFieldVisitor<IGraphType?, FieldInfo>
{
    private readonly Builder builder;

    public FieldInputVisitor(Builder builder)
    {
        this.builder = builder;
    }

    public IGraphType? Visit(IArrayField field, FieldInfo args)
    {
        if (args.Fields.Count == 0)
        {
            return null;
        }

        var schemaFieldType =
            new ListGraphType(
                new NonNullGraphType(
                    new NestedInputGraphType(builder, args)));

        return schemaFieldType;
    }

    public IGraphType? Visit(IField<AssetsFieldProperties> field, FieldInfo args)
    {
        return Scalars.Strings;
    }

    public IGraphType? Visit(IField<BooleanFieldProperties> field, FieldInfo args)
    {
        return Scalars.Boolean;
    }

    public IGraphType? Visit(IField<ComponentFieldProperties> field, FieldInfo args)
    {
        return Scalars.Json;
    }

    public IGraphType? Visit(IField<ComponentsFieldProperties> field, FieldInfo args)
    {
        return Scalars.Json;
    }

    public IGraphType? Visit(IField<DateTimeFieldProperties> field, FieldInfo args)
    {
        return Scalars.DateTime;
    }

    public IGraphType? Visit(IField<GeolocationFieldProperties> field, FieldInfo args)
    {
        return Scalars.Json;
    }

    public IGraphType? Visit(IField<JsonFieldProperties> field, FieldInfo args)
    {
        return Scalars.Json;
    }

    public IGraphType? Visit(IField<ReferencesFieldProperties> field, FieldInfo args)
    {
        return Scalars.Strings;
    }

    public IGraphType? Visit(IField<NumberFieldProperties> field, FieldInfo args)
    {
        return Scalars.Float;
    }

    public IGraphType? Visit(IField<StringFieldProperties> field, FieldInfo args)
    {
        var type = Scalars.String;

        if (field.Properties?.AllowedValues?.Count > 0 && field.Properties.CreateEnum)
        {
            var @enum = builder.GetEnumeration(args.EmbeddedEnumType, field.Properties.AllowedValues);

            if (@enum != null)
            {
                type = @enum;
            }
        }

        return type;
    }

    public IGraphType? Visit(IField<TagsFieldProperties> field, FieldInfo args)
    {
        var type = Scalars.Strings;

        if (field.Properties?.AllowedValues?.Count > 0 && field.Properties.CreateEnum)
        {
            var @enum = builder.GetEnumeration(args.EmbeddedEnumType, field.Properties.AllowedValues);

            if (@enum != null)
            {
                type = new ListGraphType(new NonNullGraphType(@enum));
            }
        }

        return type;
    }

    public IGraphType? Visit(IField<UIFieldProperties> field, FieldInfo args)
    {
        return null;
    }
}
