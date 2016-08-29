// ==========================================================================
//  ModelFieldAdded.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldAdded : IEvent
    {
        public Guid FieldId { get; set; }

        public string FieldType;

        public string FieldName { get; set; }
    }
}
