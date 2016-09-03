// ==========================================================================
//  ModelFieldAdded.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schema;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldAdded : TenantEvent
    {
        public long FieldId;

        public ModelFieldProperties Properties;
    }
}
