// ==========================================================================
//  IRegisterModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Core.Schema
{
    public interface IRegisterModelField
    {
        Type PropertiesType { get; }

        ModelField CreateField(long id, string name, IModelFieldProperties properties);
    }
}