// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Squidex.Infrastructure.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public sealed class InterfaceRegistrator<T>
        {
            private readonly IServiceCollection services;

            public InterfaceRegistrator(IServiceCollection services)
            {
                this.services = services;
            }

            public InterfaceRegistrator<T> AsSelf()
            {
                return this;
            }

            public InterfaceRegistrator<T> AsOptional<TInterface>()
            {
                if (typeof(TInterface) != typeof(T))
                {
                    services.TryAddSingleton(typeof(TInterface), c => c.GetRequiredService<T>());
                }

                return this;
            }

            public InterfaceRegistrator<T> As<TInterface>()
            {
                if (typeof(TInterface) != typeof(T))
                {
                    services.AddSingleton(typeof(TInterface), c => c.GetRequiredService<T>());
                }

                return this;
            }
        }

        public static InterfaceRegistrator<T> AddTransientAs<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : class
        {
            services.AddTransient(typeof(T), factory);

            return new InterfaceRegistrator<T>(services);
        }

        public static InterfaceRegistrator<T> AddTransientAs<T>(this IServiceCollection services) where T : class
        {
            services.AddTransient<T, T>();

            return new InterfaceRegistrator<T>(services);
        }

        public static InterfaceRegistrator<T> AddSingletonAs<T>(this IServiceCollection services, Func<IServiceProvider, T> factory) where T : class
        {
            services.AddSingleton(typeof(T), factory);

            RegisterDefaults<T>(services);

            return new InterfaceRegistrator<T>(services);
        }

        public static InterfaceRegistrator<T> AddSingletonAs<T>(this IServiceCollection services, T instance) where T : class
        {
            services.AddSingleton(typeof(T), instance);

            RegisterDefaults<T>(services);

            return new InterfaceRegistrator<T>(services);
        }

        public static InterfaceRegistrator<T> AddSingletonAs<T>(this IServiceCollection services) where T : class
        {
            services.AddSingleton<T, T>();

            RegisterDefaults<T>(services);

            return new InterfaceRegistrator<T>(services);
        }

        private static void RegisterDefaults<T>(IServiceCollection services) where T : class
        {
            if (typeof(T).GetInterfaces().Contains(typeof(IInitializable)))
            {
                services.AddSingleton(typeof(IInitializable), c => c.GetRequiredService<T>());
            }
        }
    }
}
