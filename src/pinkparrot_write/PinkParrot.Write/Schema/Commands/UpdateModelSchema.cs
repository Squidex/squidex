// ==========================================================================
//  UpdateModelSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure;

namespace PinkParrot.Write.Schema.Commands
{
    public class UpdateModelSchema
    {
        public Guid AggregateId;

        public string NewName;

        public PropertiesBag Settings { get; set; }
    }
}