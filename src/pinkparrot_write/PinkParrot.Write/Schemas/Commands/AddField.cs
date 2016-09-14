// ==========================================================================
//  AddField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace PinkParrot.Write.Schemas.Commands
{
    public class AddField : TenantCommand
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public JToken Properties { get; set; }
    }
}