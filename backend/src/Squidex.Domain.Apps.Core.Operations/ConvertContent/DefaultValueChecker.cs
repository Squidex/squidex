// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ConvertContent;

internal sealed class DefaultValueChecker : IFieldPropertiesVisitor<bool, None>
{
    private static readonly DefaultValueChecker Instance = new DefaultValueChecker();

    private DefaultValueChecker()
    {
    }

    public static bool HasDefaultValue(IField field)
    {
        Guard.NotNull(field);

        return field.RawProperties.Accept(Instance, None.Value);
    }

    public bool Visit(ArrayFieldProperties properties, None args)
    {
        return true;
    }

    public bool Visit(AssetsFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null;
    }

    public bool Visit(BooleanFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null;
    }

    public bool Visit(ComponentFieldProperties properties, None args)
    {
        return false;
    }

    public bool Visit(ComponentsFieldProperties properties, None args)
    {
        return true;
    }

    public bool Visit(DateTimeFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null || properties.CalculatedDefaultValue != null;
    }

    public bool Visit(GeolocationFieldProperties properties, None args)
    {
        return false;
    }

    public bool Visit(JsonFieldProperties properties, None args)
    {
        return false;
    }

    public bool Visit(NumberFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null;
    }

    public bool Visit(ReferencesFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null;
    }

    public bool Visit(RichTextFieldProperties properties, None args)
    {
        return false;
    }

    public bool Visit(StringFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null;
    }

    public bool Visit(TagsFieldProperties properties, None args)
    {
        return properties.DefaultValue != null || properties.DefaultValues != null;
    }

    public bool Visit(UIFieldProperties properties, None args)
    {
        return false;
    }
}
