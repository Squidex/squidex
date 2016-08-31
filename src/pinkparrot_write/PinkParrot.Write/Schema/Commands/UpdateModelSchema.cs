// ==========================================================================
//  UpdateModelSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write.Schema.Commands
{
    public class UpdateModelSchema : AggregateCommand
    {
        public string NewName;

        public PropertiesBag Settings { get; set; }
    }
}