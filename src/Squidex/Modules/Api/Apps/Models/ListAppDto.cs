// ==========================================================================
//  ListAppDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Modules.Api.Apps.Models
{
    public sealed class ListAppDto
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime LastModified { get; set; }
    }
}
