// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Schemas.Models.Fields;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Converters;

internal sealed class FieldPropertiesDtoFactory : IFieldPropertiesVisitor<FieldPropertiesDto, None>
{
    private static readonly FieldPropertiesDtoFactory Instance = new FieldPropertiesDtoFactory();

    private FieldPropertiesDtoFactory()
    {
    }

    public static FieldPropertiesDto Create(FieldProperties fieldProperties)
    {
        return fieldProperties.Accept(Instance, None.Value);
    }

    public FieldPropertiesDto Visit(ArrayFieldProperties fieldProperties, None args)
    {
        return ArrayFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(AssetsFieldProperties fieldProperties, None args)
    {
        return AssetsFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(BooleanFieldProperties fieldProperties, None args)
    {
        return BooleanFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(ComponentFieldProperties fieldProperties, None args)
    {
        return ComponentFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(ComponentsFieldProperties fieldProperties, None args)
    {
        return ComponentsFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(DateTimeFieldProperties fieldProperties, None args)
    {
        return DateTimeFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(GeolocationFieldProperties fieldProperties, None args)
    {
        return GeolocationFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(JsonFieldProperties fieldProperties, None args)
    {
        return JsonFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(NumberFieldProperties fieldProperties, None args)
    {
        return NumberFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(ReferencesFieldProperties fieldProperties, None args)
    {
        return ReferencesFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(RichTextFieldProperties fieldProperties, None args)
    {
        return RichTextFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(StringFieldProperties fieldProperties, None args)
    {
        return StringFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(TagsFieldProperties fieldProperties, None args)
    {
        return TagsFieldPropertiesDto.FromDomain(fieldProperties);
    }

    public FieldPropertiesDto Visit(UIFieldProperties fieldProperties, None args)
    {
        return UIFieldPropertiesDto.FromDomain(fieldProperties);
    }
}
