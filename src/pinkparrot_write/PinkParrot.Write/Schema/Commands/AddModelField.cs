// ==========================================================================
//  AddModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Write.Schema.Commands
{
    public class AddModelField
    {
        public Guid AggregateId;

        public string FieldType;

        public string FieldName;
    }
}