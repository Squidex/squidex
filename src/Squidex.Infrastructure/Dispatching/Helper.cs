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
        public static bool HasMatchingName(this MethodInfo method, string name)
        {
            return string.Equals(method.Name, name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasMatchingReturnType(this MethodInfo method, Type type)
        {
            return method.ReturnType == type;
        }

        public static bool HasMatchingParameters<TIn>(this MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 1 && typeof(TIn).IsAssignableFrom(parameters[0].ParameterType);
        }

        public static bool HasMatchingParameters<TIn, TContext>(this MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 2 && typeof(TIn).IsAssignableFrom(parameters[0].ParameterType) && parameters[1].ParameterType == typeof(TContext);
        }
    }
}