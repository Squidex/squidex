// ==========================================================================
//  EnableField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Write.Schemas.Commands
{
    public class EnableField : TenantCommand
    {
        public long FieldId { get; set; }
    }
}
