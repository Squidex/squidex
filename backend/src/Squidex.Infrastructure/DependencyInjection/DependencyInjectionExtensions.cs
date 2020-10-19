// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Squidex.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public delegate void Registrator(Type serviceType, Func<IServiceProvider, object> implementationFactory);

        public sealed class InterfaceRegistrator<T> where T : notnull
        {
            private readonly Registrator register;
            private readonly Registrator registerOptional;

            public InterfaceRegistrator(Registrator register, Registrator registerOptional)
            {
                this.register = register;
                this.registerOptional = registerOptional;

                var interfaces = typeof(T).GetInterfaces();

                if (interfaces.Contains(typeof(IInitializable)))
                {
                    register(typeof(IInitializable), c => c.GetRequiredService<T>());
                }

                if (interfaces.Contains(typeof(IBackgroundProcess)))
                {
                    register(typeof(IBackgroundProcess), c => c.GetRequiredService<T>());
                }
            }

            public InterfaceRegistrator<T> AsSelf()
            {
                return this;
            }

            public InterfaceRegistrator<T> AsOptional<TInterface>()
            {
                if (typeof(TInterface) != typeof(T))
                {
                    registerOptional(typeof(TInterface), c => c.GetRequiredService<T>());
                }

                return this;
            }

            public InterfaceRegistrator<T> As<TInterface>()
            {
                if (typeof(TInterface) != typeof(T))
                {
                    register(typeof(TInterface), c => c.GetRequiredService<T>());
                }

                return this;
            }
        }

        public static InterfaceRegistrator<T> AddTransientAs<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : class
        {
            services.AddTransient(typeof(T), factory);

            return new InterfaceRegistrator<T>((t, f) => services.AddTransient(t, f), services.TryAddTransient);
        }

        public static InterfaceRegistrator<T> AddTransientAs<T>(this IServiceCollection services) where T : class
        {
            services.AddTransient<T, T>();

            return new InterfaceRegistrator<T>((t, f) => services.AddTransient(t, f), services.TryAddTransient);
        }

        public static InterfaceRegistrator<T> AddSingletonAs<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : class
        {
            services.AddSingleton(typeof(T), factory);

            return new InterfaceRegistrator<T>((t, f) => services.AddSingleton(t, f), services.TryAddSingleton);
        }

        public static InterfaceRegistrator<T> AddSingletonAs<T>(this IServiceCollection services) where T : class
        {
            services.AddSingleton<T, T>();

            return new InterfaceRegistrator<T>((t, f) => services.AddSingleton(t, f), services.TryAddSingleton);
        }

        public static InterfaceRegistrator<T> AddScopedAs<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : class
        {
            services.AddScoped(typeof(T), factory);

            return new InterfaceRegistrator<T>((t, f) => services.AddScoped(t, f), services.TryAddScoped);
        }

        public static InterfaceRegistrator<T> AddScopedAs<T>(this IServiceCollection services) where T : class
        {
            services.AddScoped<T, T>();

            return new InterfaceRegistrator<T>((t, f) => services.AddScoped(t, f), services.TryAddScoped);
        }
    }
}
