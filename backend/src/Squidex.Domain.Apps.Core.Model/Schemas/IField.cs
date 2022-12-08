// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas;

public interface IField : IFieldSettings
{
    long Id { get; }

    string Name { get; }

    FieldProperties RawProperties { get; }

    T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, TArgs args);
}
