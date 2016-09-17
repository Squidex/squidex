// ==========================================================================
//  IRegisteredField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Core.Schemas
{
    public interface IRegisteredField
    {
        Type PropertiesType { get; }

        Field CreateField(long id, string name, FieldProperties properties);
    }
}