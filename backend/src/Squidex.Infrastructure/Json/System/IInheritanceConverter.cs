// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Json.System;

public interface IInheritanceConverter
{
    string DiscriminatorName { get; }

    Type GetDiscriminatorType(string name, Type typeToConvert);

    string GetDiscriminatorValue(Type type);
}
