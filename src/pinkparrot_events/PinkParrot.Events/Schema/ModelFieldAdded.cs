// ==========================================================================
//  ModelFieldAdded.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Events.Schema
{
    public class ModelFieldAdded : TenantEvent
    {
        public long FieldId;

        public string FieldType;

        public string FieldName;
    }
}
