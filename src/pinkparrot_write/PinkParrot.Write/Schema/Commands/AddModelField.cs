// ==========================================================================
//  AddModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace PinkParrot.Write.Schema.Commands
{
    public class AddModelField : TenantCommand
    {
        public string Name;

        public string Type;

        public JToken Properties;
    }
}