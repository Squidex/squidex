// ==========================================================================
//  Helper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;

namespace Squidex.Infrastructure.Dispatching
{
    internal static class Helper
    {
        public static bool HasRightName(MethodInfo method)
        {
            return string.Equals(method.Name, "On", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasRightReturnType<TOut>(MethodInfo method)
        {
            return method.ReturnType == typeof(TOut);
        }

        public static bool HasRightParameters<TIn>(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 1 && typeof(TIn).IsAssignableFrom(parameters[0].ParameterType);
        }

        public static bool HasRightParameters<TIn, TContext>(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 2 && typeof(TIn).IsAssignableFrom(parameters[0].ParameterType) && parameters[1].ParameterType == typeof(TContext);
        }
    }
}