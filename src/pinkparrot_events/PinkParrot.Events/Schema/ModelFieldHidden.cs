// ==========================================================================
//  ModelFieldHidden.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS;

namespace PinkParrot.Events.Schema
{
    public class ModelFieldHidden : IEvent
    {
        public Guid FieldId;
    }
}
