// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;

namespace Squidex.Config
{
    public static class ServiceExtensions
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

        public static T GetOptionalValue<T>(this IConfiguration config, string path, T defaultValue = default)
        {
            var value = config.GetValue(path, defaultValue);

            return value;
        }

        public static int GetOptionalValue(this IConfiguration config, string path, int defaultValue)
        {
            var value = config.GetValue<string>(path);
            var result = defaultValue;

            if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }

        public static string GetRequiredValue(this IConfiguration config, string path)
        {
            var value = config.GetValue<string>(path);

            if (string.IsNullOrWhiteSpace(value))
            {
                var name = string.Join(' ', path.Split(':').Select(x => x.ToPascalCase()));

                throw new ConfigurationException($"Configure the {name} with '{path}'.");
            }

            return value;
        }

        public static string ConfigureByOption(this IConfiguration config, string path, Options options)
        {
            var value = config.GetRequiredValue(path);

            if (options.TryGetValue(value, out var action))
            {
                action();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{value}' for '{path}', supported: {string.Join(' ', options.Keys)}.");
            }

            return value;
        }
    }
}
