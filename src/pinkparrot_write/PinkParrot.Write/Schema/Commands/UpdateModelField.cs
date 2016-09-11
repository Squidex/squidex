// ==========================================================================
//  UpdateModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace PinkParrot.Write.Schema.Commands
{
    public class UpdateModelField : TenantCommand
    {
        public long FieldId;

        public JToken Properties { get; set; }
    }
}