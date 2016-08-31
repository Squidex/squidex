// ==========================================================================
//  ModelFieldFactory.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Core.Schema
{
    public class ModelFieldFactory
    {
        public virtual ModelField CreateField(long id, string type, string fieldName)
        {
            return new NumberField(id, fieldName);
        }
    }
}

