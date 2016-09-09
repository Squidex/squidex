// ==========================================================================
//  AddModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schema;

namespace PinkParrot.Write.Schema.Commands
{
    public class AddModelField : TenantCommand
    {
        public ModelFieldProperties Properties { get; set; }
    }
}