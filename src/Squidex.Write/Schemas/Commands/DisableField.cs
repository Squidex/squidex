// ==========================================================================
//  DisableField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Write.Schemas.Commands
{
    public class DisableField : AppCommand
    {
        public long FieldId { get; set; }
    }
}
