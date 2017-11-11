// ==========================================================================
//  ServiceExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
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

            public InterfaceRegistrator<T> As<TInterface>()
            {
                this.services.AddSingleton(typeof(TInterface), c => c.GetRequiredService<T>());

                return this;
            }
        }

        public static InterfaceRegistrator<T> AddSingleton<T>(this IServiceCollection services, Func<IServiceProvider, T> factory)
        {
            services.AddSingleton(typeof(T), factory);

            return new InterfaceRegistrator<T>(services);
        }

        public static InterfaceRegistrator<T> AddSingleton<T>(this IServiceCollection services, T instance)
        {
            services.AddSingleton(instance);

            return new InterfaceRegistrator<T>(services);
        }

        public static InterfaceRegistrator<T> AddSingleton<T>(this IServiceCollection services)
        {
            services.AddSingleton(typeof(T));

            return new InterfaceRegistrator<T>(services);
        }

        public static T GetOptionalValue<T>(this IConfiguration config, string path, T defaultValue = default(T))
        {
            var value = config.GetValue<T>(path, defaultValue);

            return value;
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
