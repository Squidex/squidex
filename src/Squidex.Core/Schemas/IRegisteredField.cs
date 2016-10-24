// ==========================================================================
//  IRegisteredField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Core.Schemas
{
    public interface IRegisteredField
    {
        Type PropertiesType { get; }

        Field CreateField(long id, string name, FieldProperties properties);
    }
}