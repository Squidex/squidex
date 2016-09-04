// ==========================================================================
//  CreateModelSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write.Schema.Commands
{
    public class CreateModelSchema : AggregateCommand
    {
        public ModelSchemaProperties Properties { get; set; }
    }
}