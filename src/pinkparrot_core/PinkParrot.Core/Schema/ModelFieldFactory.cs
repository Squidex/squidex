// ==========================================================================
//  ModelFieldFactory.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Core.Schema
{
    public class ModelFieldFactory
    {
        public virtual ModelField CreateField(Guid id, string type, string fieldName)
        {
            return new NumberField(id, fieldName);
        }
    }
}

