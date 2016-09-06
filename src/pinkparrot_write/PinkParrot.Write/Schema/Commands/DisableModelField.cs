// ==========================================================================
//  DisableModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Write.Schema.Commands
{
    public class DisableModelField : TenantCommand
    {
        public long FieldId { get; set; }
    }
}
