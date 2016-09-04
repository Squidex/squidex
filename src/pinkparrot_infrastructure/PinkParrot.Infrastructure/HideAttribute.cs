// ==========================================================================
//  HideAttribute.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HideAttribute : Attribute
    {
    }
}