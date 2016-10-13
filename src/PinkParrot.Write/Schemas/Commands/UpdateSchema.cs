// ==========================================================================
//  UpdateSchema.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;

namespace PinkParrot.Write.Schemas.Commands
{
    public class UpdateSchema : AppCommand
    {
        public SchemaProperties Properties { get; set; }
    }
}