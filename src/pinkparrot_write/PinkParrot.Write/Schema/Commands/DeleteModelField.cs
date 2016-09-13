// ==========================================================================
//  DeleteModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

namespace PinkParrot.Write.Schema.Commands
{
    public class DeleteModelField : TenantCommand
    {
        public long FieldId { get; set; }
    }
}