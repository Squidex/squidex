// ==========================================================================
//  DescribedResponseTypeAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc;

namespace Squidex.Pipeline
{
    public sealed class DescribedResponseTypeAttribute : ProducesResponseTypeAttribute
    {
        public string Description { get; }

        public DescribedResponseTypeAttribute(int statusCode, string description = null)
            : base(typeof(void), statusCode)
        {
            Description = description;
        }

        public DescribedResponseTypeAttribute(int statusCode, Type type, string description = null) 
            : base(type, statusCode)
        {
            Description = description;
        }
    }
}
