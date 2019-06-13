﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public interface IFieldPropertiesVisitor<out T>
    {
        T Visit(ArrayFieldProperties properties);

        T Visit(AssetsFieldProperties properties);

        T Visit(BooleanFieldProperties properties);

        T Visit(DateTimeFieldProperties properties);

        T Visit(GeolocationFieldProperties properties);

        T Visit(JsonFieldProperties properties);

        T Visit(NumberFieldProperties properties);

        T Visit(ReferencesFieldProperties properties);

        T Visit(StringFieldProperties properties);

        T Visit(TagsFieldProperties properties);

        T Visit(UIFieldProperties properties);
    }
}
