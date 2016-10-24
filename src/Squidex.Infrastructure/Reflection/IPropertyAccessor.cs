// ==========================================================================
//  IPropertyAccessor.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Infrastructure.Reflection
{
    public interface IPropertyAccessor
    {
        object Get(object target);
        
        void Set(object target, object value);
    }
}
