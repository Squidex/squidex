// ==========================================================================
//  UpdateField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace PinkParrot.Write.Schemas.Commands
{
    public class UpdateField : AppCommand
    {
        public long FieldId { get; set; }

        public JToken Properties { get; set; }
    }
}