// ==========================================================================
//  Helper.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;

namespace PinkParrot.Infrastructure.Dispatching
{
    internal static class Helper
    {
        public static bool HasRightName(MethodInfo method)
        {
            return method.Name == "On";
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