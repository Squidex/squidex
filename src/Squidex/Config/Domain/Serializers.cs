// ==========================================================================
//  Serializers.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Config.Domain
{
    public static class Serializers
    {
        private static readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();

        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings, TypeNameHandling typeNameHandling)
        {
            settings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new LanguageConverter());
            settings.Converters.Add(new PropertiesBagConverter());
            settings.Converters.Add(new RefTokenConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTime;
            settings.TypeNameHandling = typeNameHandling;

            return settings;
        }

        static Serializers()
        {
            typeNameRegistry.Map(typeof(EventExtensions).GetTypeInfo().Assembly);
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return ConfigureJson(new JsonSerializerSettings(), TypeNameHandling.Auto);
        }

        private static JsonSerializer CreateSerializer(JsonSerializerSettings settings)
        {
            return JsonSerializer.Create(settings);
        }

        public static IServiceCollection AddMyEventFormatter(this IServiceCollection services)
        {
            services.AddSingleton(t => typeNameRegistry);
            services.AddSingleton(t => CreateSettings());
            services.AddSingleton(t => CreateSerializer(t.GetRequiredService<JsonSerializerSettings>()));

            return services;
        }

        public static IMvcBuilder AddMySerializers(this IMvcBuilder mvc)
        {
            mvc.AddJsonOptions(options =>
            {
                ConfigureJson(options.SerializerSettings, TypeNameHandling.None);
            });

            return mvc;
        }
    }
}
