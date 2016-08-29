// ==========================================================================
//  UpdateModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure;

namespace PinkParrot.Write.Schema.Commands
{
    public class UpdateModelField
    {
        public Guid AggregateId;

        public Guid FieldId;

        public PropertiesBag Settings;
    }
}