// ==========================================================================
//  ShowField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Write.Schemas.Commands
{
    public class ShowField : TenantCommand
    {
        public long FieldId { get; set; }
    }
}
