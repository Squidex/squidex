// ==========================================================================
//  ModelFieldDeleted.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Events.Schema
{
    public class ModelFieldDeleted : TenantEvent
    {
        public long FieldId;
    }
}
