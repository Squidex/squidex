// ==========================================================================
//  ModelFieldAdded.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldAdded : IEvent
    {
        public long FieldId;

        public string FieldType;

        public string FieldName;
    }
}
